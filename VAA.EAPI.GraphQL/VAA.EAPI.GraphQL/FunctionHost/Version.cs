namespace VAA.EAPI.GraphQL.FunctionHost
{
    public sealed class Version
    {
        public static explicit operator Version(System.Version source) => new Version(source);

        private readonly System.Version version;

        private Version(System.Version version) => this.version = version;

        public int Major => version.Major;

        public int Minor => version.Minor;

        public int Build => version.Build;

        public int Revision => version.Revision;

        public short MajorRevision => version.MajorRevision;

        public short MinorRevision => version.MinorRevision;
    }
}
