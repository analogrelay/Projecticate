namespace Projecticate
{
    public class ProjectedDirectoryEntry
    {
        public string FileName { get; }
        public FileBasicInfo BasicInfo { get; }

        public ProjectedDirectoryEntry(string fileName, FileBasicInfo basicInfo)
        {
            FileName = fileName;
            BasicInfo = basicInfo;
        }
    }
}