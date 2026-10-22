using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Common.Http
{
    public sealed class CommandParser : IDisposable
    {
        private const string SubCallPattern = @"Arg\d+=""<[^/>]+/>""";
        private const string SubCallCommandPattern = @"(?<=Arg\d+\=""<)[^/>]+";
        private const string LoopForPattern = @"\{Loop\((\d+)\)\}";
        private const string LoopForEndPattern = @"\{Loop\}";
        private const string _xslt = @"<?xml version=""1.0""?>
<xsl:stylesheet version=""1.0"" 
                xmlns=""http://www.w3.org/1999/xhtml""
                xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
                xmlns:methods=""urn:methods""
                xmlns:fn=""http://www.w3.org/2005/xpath-functions""
                exclude-result-prefixes=""xsl methods fn"">
    <xsl:output encoding=""utf-8"" omit-xml-declaration=""yes"" method=""html"" indent=""yes""/>
      <xsl:template match=""/Response"">
        <response>

            <xsl:for-each select=""*|text()"">
                <xsl:call-template name=""ApplyCommands"" />
            </xsl:for-each>

        </response>
    </xsl:template>

    <xsl:template name=""ApplyCommands"">
        <xsl:param name=""index"">0</xsl:param>

        <xsl:choose>
            <xsl:when test=""local-name()='Guid'"">
                <xsl:value-of select=""methods:Guid()""/>
            </xsl:when>
            <xsl:when test=""local-name()='RandomNumber'"">
                <xsl:choose>
                    <xsl:when test=""boolean(@Arg1)"">
                        <xsl:value-of select=""methods:RandomNumber(@Arg0, @Arg1)""/>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select=""methods:RandomNumber(@Arg0)""/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:when test=""local-name()='RandomDateTime'"">
                <xsl:value-of select=""methods:RandomDateTime()""/>
            </xsl:when>
            <xsl:when test=""local-name()='RandomString'"">
                <xsl:choose>
                    <xsl:when test=""boolean(@Arg1)"">
                        <xsl:value-of select=""methods:RandomString(@Arg0, @Arg1)""/>
                    </xsl:when>
                    <xsl:when test=""boolean(@Arg0)"">
                        <xsl:value-of select=""methods:RandomString(@Arg0)""/>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select=""methods:RandomString()""/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:when test=""local-name()='DateTime'"">
                <xsl:choose>
                    <xsl:when test=""boolean(@Arg2)"">
                        <xsl:value-of select=""methods:DateTime(@Arg0, @Arg1, @Arg2)""/>
                    </xsl:when>
                    <xsl:when test=""boolean(@Arg1)"">
                        <xsl:value-of select=""methods:DateTime(@Arg0, @Arg1)""/>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select=""methods:DateTime(@Arg0)""/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:when test=""local-name()='RandomItem'"">
                <xsl:value-of select=""methods:RandomItem(.)""/>
            </xsl:when>
            <xsl:when test=""local-name()='Index'"">
                <xsl:value-of select=""$index""/>
            </xsl:when>
            <xsl:when test=""local-name()='Loop'"">
                <xsl:variable name=""for"" select=""@for""/>
                <xsl:call-template name=""Loop"">
                    <xsl:with-param name=""index"" select=""$for""/>
                    <xsl:with-param name=""nodes"" select=""*|text()""/>
                </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy>
                    <xsl:apply-templates select=""."">
                        <xsl:with-param name=""index"" select=""$index""/>
                    </xsl:apply-templates>
                </xsl:copy>
            </xsl:otherwise>
        </xsl:choose>

    </xsl:template>

    <xsl:template name=""Loop"" match=""*|text()"" mode=""iter"">
        <xsl:param name=""index"" select=""0""/>
        <xsl:param name=""nodes"" select=""*|text()""/>

        <xsl:if test=""$index > 0"">
            <xsl:for-each select=""$nodes"">
                <xsl:call-template name=""ApplyCommands"">
                    <xsl:with-param name=""index"" select=""$index""/>
                </xsl:call-template>
            </xsl:for-each>
            <xsl:apply-templates select=""."" mode=""iter"">
                <xsl:with-param name=""index"" select=""$index - 1""/>
            </xsl:apply-templates>
        </xsl:if>
    </xsl:template>

</xsl:stylesheet>";

        private static readonly XsltArgumentList _xsltArgumentList = new();
        private static readonly XslCompiledTransform _xslCompiledTransform = new(Debugger.IsAttached);

        public static explicit operator CommandParser(string? source) => new(source);

        static CommandParser()
        {
            _xsltArgumentList.AddExtensionObject("urn:methods", new Methods());

            using var xsltStringReader = new StringReader(_xslt);
            using var xsltXmlReader = XmlReader.Create(xsltStringReader);

            _xslCompiledTransform.Load(xsltXmlReader);
        }

        internal sealed class Command
        {
            private const string NamePattern = @"(?<={)\w+";
            private const string ArgumentPattern = @"(?<=\()[^\)]+";

            public static explicit operator Command(string source) => new(source);

            public string Pattern { get; }
            public string Name { get; }


            public Command(string pattern)
            {
                Pattern = pattern;
                Name = RegexCompiled.Match(pattern, NamePattern).Value;
            }

            public string Parse(string value)
            {
                if (value.IsNullOrWhiteSpace()) return string.Empty;

                try
                {
                    if (!Commands.Has(value, this)) return value;

                    foreach (Match match in RegexCompiled.Matches(value, Pattern))
                    {
                        var entry = match.Value;

                        var arguments = RegexCompiled.Match(entry, ArgumentPattern).Value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        string xml = $"<{Name} {string.Join(' ', arguments.Select((x, i) => $@"Arg{i}=""{x.Trim()}"""))}/>";

                        value = RegexCompiled.Replace(value, Regex.Escape(entry), xml);

                        if (!Commands.Has(value, this)) break;
                    }

                }
                catch (Exception ex)
                {
                }

                return value;
            }
        }

        internal sealed class Commands : IEnumerable<Command>
        {
            const string Guid = @"\{Guid\(\)\}";
            const string DateTime = @"\{DateTime\([^\)]*\)\}";           // 1 parameter format, second parameter +/- addition, third parameter addition type millisecond,second,minute,hour,day(default),month,year,
            const string RandomNumber = @"\{RandomNumber\([^\)]*\)\}";       // 2 parameters ~ range, above 2 items is a list
            const string RandomDateTime = @"\{RandomDateTime\([^\)]*\)\}";   // 2 parameters ~ range, above 2 items is a list, 0 parameters random date
            const string RandomString = @"\{RandomString\([^\)]*\)\}";       // 1 parameter format e.g. 'testdata_{0}', no parameter random string
            const string RandomItem = @"\{RandomItem\([^\)]+\)\}";           // 2 parameters ~ range, above 2 items is a list
            const string Index = @"\{Index\(\)\}";

            public static IEnumerable<Command> All => new Commands();

            public static bool HasAny(string template) => All.Any(x => Has(template, x));
            public static bool Has(string template, Command command) => RegexCompiled.IsMatch(template, command.Pattern, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);


            private Commands() { }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerator<Command> GetEnumerator()
            {
                yield return (Command)Guid;
                yield return (Command)DateTime;
                yield return (Command)RandomNumber;
                yield return (Command)RandomDateTime;
                yield return (Command)RandomString;
                yield return (Command)RandomItem;
                yield return (Command)Index;
            }
        }

        internal sealed class Methods()
        {
            private const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            private const string millisecond = "millisecond";
            private const string second = "second";
            private const string minute = "minute";
            private const string hour = "hour";
            private const string day = "day";
            private const string week = "week";
            private const string month = "month";
            private const string year = "year";


            private readonly Random _random = new();
            private readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

            public string Guid() => System.Guid.NewGuid().ToString();

            public string DateTime(string format) => DateTime(format, 0, string.Empty);
            public string DateTime(string format, int offset) => DateTime(format, offset, day);
            public string DateTime(string format, int offset, string offsetType) => offsetType.Trim().ToLower() switch
            {
                millisecond => System.DateTime.Now.AddMilliseconds(offset).ToString(format),
                second => System.DateTime.Now.AddSeconds(offset).ToString(format),
                minute => System.DateTime.Now.AddMinutes(offset).ToString(format),
                hour => System.DateTime.Now.AddHours(offset).ToString(format),
                day => System.DateTime.Now.AddDays(offset).ToString(format),
                week => System.DateTime.Now.AddDays(offset * 7).ToString(format),
                month => System.DateTime.Now.AddMonths(offset).ToString(format),
                year => System.DateTime.Now.AddYears(offset).ToString(format),
                _ => System.DateTime.Now.ToString(format)
            };

            public string RandomNumber(int max) => RandomNumber(0, max);
            public string RandomNumber(int min, int max) => _random.Next(min, max).ToString();

            public string RandomDateTime() => System.DateTime.Now.AddYears(_random.Next(0, 1) == 0 ? _random.Next(0, 100) : -_random.Next(0, 100))
                                                                 .AddMonths(_random.Next(0, 1) == 0 ? _random.Next(0, 100) : -_random.Next(0, 100))
                                                                 .AddDays(_random.Next(0, 1) == 0 ? _random.Next(0, 100) : -_random.Next(0, 100))
                                                                 .AddHours(_random.Next(0, 1) == 0 ? _random.Next(0, 100) : -_random.Next(0, 100))
                                                                 .AddMinutes(_random.Next(0, 1) == 0 ? _random.Next(0, 100) : -_random.Next(0, 100))
                                                                 .AddSeconds(_random.Next(0, 1) == 0 ? _random.Next(0, 100) : -_random.Next(0, 100))
                                                                 .ToString("yyyy-MM-ddTHH:mm:ss.fff");

            public string RandomString() => RandomString(RandomNumber(32).Parse<int>(), "{0}");
            public string RandomString(int length) => RandomString(length, "{0}");
            public string RandomString(int length, string format)
            {
                byte[] data = new byte[length];

                _randomNumberGenerator.GetBytes(data);

                return string.Format(format, string.Join(string.Empty, data.Select(x => chars[x % chars.Length])));
            }

            public string RandomItem(XPathNavigator node)
            {
                List<string> arguments = [];

                if (node.MoveToFirstAttribute())
                {
                    arguments.Add(node.Value);

                    while (node.MoveToNextAttribute())
                        arguments.Add(node.Value);
                }

                return arguments[RandomNumber(arguments.Count).Parse<int>()];
            }

            public string Execute(string command)
            {
                var arguments = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = arguments[0];

                arguments = [.. arguments.Skip(1).Select(x => x.Split('=')[1].TrimStart('"').TrimEnd('"'))];

                if (name.Is(nameof(Guid))) return Guid();

                if (name.Is(nameof(DateTime)))
                    return arguments.Length switch
                    {
                        3 => DateTime(arguments[0], arguments[1].Parse<int>(), arguments[2]),
                        2 => DateTime(arguments[0], arguments[1].Parse<int>()),
                        1 => DateTime(arguments[0]),
                        _ => throw new NotImplementedException()
                    };

                if (name.Is(nameof(RandomNumber)))
                    return arguments.Length switch
                    {
                        2 => RandomNumber(arguments[0].Parse<int>(), arguments[1].Parse<int>()),
                        1 => RandomNumber(arguments[0].Parse<int>()),
                        _ => throw new NotImplementedException()
                    };

                if (name.Is(nameof(RandomDateTime)))
                    return arguments.Length switch
                    {
                        2 => RandomNumber(arguments[0].Parse<int>(), arguments[1].Parse<int>()),
                        1 => RandomNumber(arguments[0].Parse<int>()),
                        _ => RandomDateTime(),
                    };

                if (name.Is(nameof(RandomString)))
                    return arguments.Length switch
                    {
                        2 => RandomString(arguments[0].Parse<int>(), arguments[1]),
                        1 => RandomString(arguments[0].Parse<int>()),
                        _ => RandomString(),
                    };

                throw new NotImplementedException();
            }
        }

        public string? Template { get; private set; }

        public CommandParser(string? template)
        {
            if (template.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(template));

            Template = template!.Trim();
        }

        public string Parse()
        {
            if (Template.IsNullOrWhiteSpace()) return string.Empty;

            bool hasCommands = Commands.HasAny(Template!);
            bool hasLoops = RegexCompiled.IsMatch(Template!, LoopForPattern) ||
                RegexCompiled.IsMatch(Template!, LoopForEndPattern);

            if (!hasCommands && !hasLoops)
                return Template!;

            string parsedTemplate = WebUtility.HtmlEncode(Template!);

            if (hasCommands)
            {
                foreach (var command in Commands.All)
                    parsedTemplate = command.Parse(parsedTemplate);

                var subCalls = RegexCompiled.Matches(parsedTemplate, SubCallPattern);

                if (subCalls.Count != 0)
                {
                    var methods = (Methods)_xsltArgumentList.GetExtensionObject("urn:methods")!;

                    foreach (Match subCall in subCalls)
                    {
                        if (!parsedTemplate.Contains(subCall.Value)) continue;

                        var result = methods.Execute(RegexCompiled.Match(subCall.Value, SubCallCommandPattern).Value.Trim());
                        parsedTemplate = RegexCompiled.Replace(parsedTemplate, subCall.Value, $@"{subCall.Value[..subCall.Value.IndexOf('=')]}=""{result}""");
                    }
                }
            }

            if (RegexCompiled.IsMatch(parsedTemplate, LoopForPattern))
            {
                parsedTemplate = RegexCompiled.Replace(parsedTemplate, LoopForPattern, @"<Loop for=""$1"">");
                parsedTemplate = RegexCompiled.Replace(parsedTemplate, LoopForEndPattern, "</Loop>");
            }

            var stringBuilder = new StringBuilder();

            using var xmlStringReader = new StringReader($"<Response>{parsedTemplate}</Response>");
            using var templateXmlReader = XmlReader.Create(xmlStringReader);
            using var xmlWriter = XmlWriter.Create(stringBuilder);

            _xslCompiledTransform.Transform(templateXmlReader, _xsltArgumentList, xmlWriter);

            return WebUtility.HtmlDecode(stringBuilder.ToString().Substring(86, stringBuilder.Length - 86 - 11));
        }

        public void Dispose() => Template = null;
    }
}
