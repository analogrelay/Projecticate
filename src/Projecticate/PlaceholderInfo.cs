namespace Projecticate
{
    public class PlaceholderInfo
    {
        public FileBasicInfo BasicInfo { get; }
        public PlaceholderVersionInfo VersionInfo { get; set; }

        public PlaceholderInfo(FileBasicInfo basicInfo)
        {
            BasicInfo = basicInfo;
        }
    }
}
