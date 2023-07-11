namespace VersionSourceGenerator.UnitTest;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Assert.Equal("1.2.3", Version.Full);
        Assert.Equal("1.2.3", Version.Core);
        Assert.Equal(1, Version.Major);
        Assert.Equal(2, Version.Minor);
        Assert.Equal(3, Version.Patch);
        Assert.False(Version.IsPrerelease);
        Assert.Equal("", Version.Prerelease);
        Assert.Equal("", Version.Metadata);
        Assert.Empty(Version.PrereleaseIdentifiers);
        Assert.Empty(Version.MetadataIdentifiers);

        Assert.Equal("2.3.4-alpha.5", Platform1Version.Full);
        Assert.Equal("2.3.4", Platform1Version.Core);
        Assert.Equal(2, Platform1Version.Major);
        Assert.Equal(3, Platform1Version.Minor);
        Assert.Equal(4, Platform1Version.Patch);
        Assert.True(Platform1Version.IsPrerelease);
        Assert.Equal("alpha.5", Platform1Version.Prerelease);
        Assert.Equal("alpha", Platform1Version.PrereleaseIdentifiers[0]);
        Assert.Equal("5", Platform1Version.PrereleaseIdentifiers[1]);
        Assert.Equal("", Platform1Version.Metadata);
        Assert.Empty(Platform1Version.MetadataIdentifiers);

        Assert.Equal("3.4.5+build.6", Platform2Version.Full);
        Assert.Equal("3.4.5", Platform2Version.Core);
        Assert.Equal(3, Platform2Version.Major);
        Assert.Equal(4, Platform2Version.Minor);
        Assert.Equal(5, Platform2Version.Patch);
        Assert.False(Platform2Version.IsPrerelease);
        Assert.Empty(Platform2Version.PrereleaseIdentifiers);
        Assert.Equal("build.6", Platform2Version.Metadata);
        Assert.Equal("build", Platform2Version.MetadataIdentifiers[0]);
        Assert.Equal("6", Platform2Version.MetadataIdentifiers[1]);

        Assert.Equal("4.5.6-alpha.7+build.8", Platform3Version.Full);
        Assert.Equal("4.5.6", Platform3Version.Core);
        Assert.Equal(4, Platform3Version.Major);
        Assert.Equal(5, Platform3Version.Minor);
        Assert.Equal(6, Platform3Version.Patch);
        Assert.True(Platform3Version.IsPrerelease);
        Assert.Equal("alpha.7", Platform3Version.Prerelease);
        Assert.Equal("alpha", Platform3Version.PrereleaseIdentifiers[0]);
        Assert.Equal("7", Platform3Version.PrereleaseIdentifiers[1]);
        Assert.Equal("build.8", Platform3Version.Metadata);
        Assert.Equal("build", Platform3Version.MetadataIdentifiers[0]);
        Assert.Equal("8", Platform3Version.MetadataIdentifiers[1]);
    }
}