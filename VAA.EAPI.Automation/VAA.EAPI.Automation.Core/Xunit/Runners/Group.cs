using System.Text;
using VAA.EAPI.Automation.Core.Extensions;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Xunit.Runners
{
    internal sealed class Group : IOperation
    {
        public static explicit operator Group(string source)
        {
            Group root = new();
            Group current = root;
            string currentCondition = string.Empty;

            for (int i = 0; i < source.Length; i++)
            {
                char @char = source[i];

                switch (@char)
                {
                    case '(':

                        currentCondition = string.Empty;

                        current = new Group()
                        {
                            Parent = current
                        };

                        break;
                    case ')':

                        currentCondition = string.Empty;

                        current = current.Parent;

                        break;
                    case '&':
                    case '|':

                        if (!currentCondition.IsNullOrWhiteSpace())
                        {
                            current.Operations = current.Operations.Append((Condition)(current, currentCondition));

                            currentCondition = string.Empty;
                        }

                        current.Operations = current.Operations.Append((Operator)@char.ToString());

                        break;
                    default:

                        currentCondition += @char;

                        break;
                }
            }

            if (!currentCondition.IsNullOrWhiteSpace())
                current.Operations = current.Operations.Append((Condition)(current, currentCondition));

            return root;
        }

        public Group Parent { get; set; }
        public IEnumerable<IOperation> Operations { get; internal set; } = Enumerable.Empty<IOperation>();

        public bool IsValid(ITestCase testCase)
        {
            if (Operations.Any(x => x is Operator))
            {
                Operator[] operators = Operations.Where(x => x is Operator)
                                          .Select(x => (Operator)x)
                                          .ToArray();
                Condition[] conditions = Operations.Where(x => x is Condition)
                                           .Select(x => (Condition)x)
                                           .ToArray();
                Group[] groups = Operations.Where(x => x is Group)
                                           .Select(x => (Group)x)
                                           .ToArray();

                bool justOr = operators.All(x => x.Value.Is("|"));
                bool justAnd = operators.All(x => x.Value.Is("&"));

                if (justOr)
                    return conditions.Any(x => x.IsValid(testCase)) || groups.Any(x => x.IsValid(testCase));

                if (justAnd)
                    return conditions.All(x => x.IsValid(testCase)) && groups.All(x => x.IsValid(testCase));

                if (!justOr && !justAnd)
                {
                    // tobe implenented
                }
            }

            return Operations.All(x => ((Condition)x).IsValid(testCase));
        }
    }
}
