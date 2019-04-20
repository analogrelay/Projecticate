using System;
using System.Runtime.InteropServices;

namespace Projecticate
{
    internal static class NativeMethods
    {
        public const uint HRESULT_ERROR_INSUFFICIENT_BUFFER = 0x8007007A;

        [DllImport("projectedfslib.dll", CharSet = CharSet.Unicode)]
        public static extern uint PrjMarkDirectoryAsPlaceholder(
            string rootPathName,
            string targetPathName,
            IntPtr versionInfo,
            in Guid virtualizationInstanceId);

        [DllImport("projectedfslib.dll", CharSet = CharSet.Unicode)]
        public static extern uint PrjStartVirtualizing(
            string virtualizationRootPath,
            in PRJ_CALLBACKS callbacks,
            IntPtr instanceContext,
            IntPtr options,
            out IntPtr namespaceVirtualizationContext);

        [DllImport("projectedfslib.dll", CharSet = CharSet.Unicode)]
        public static extern uint PrjFillDirEntryBuffer(
            string fileName,
            in PRJ_FILE_BASIC_INFO fileBasicInfo,
            IntPtr dirEntryBufferHandle);

        [DllImport("projectedfslib.dll", CharSet = CharSet.Unicode)]
        public static extern uint PrjWritePlaceholderInfo(
            IntPtr namespaceVirtualizationContext,
            [MarshalAs(UnmanagedType.LPWStr)]
            string destinationFileName,
            in PRJ_PLACEHOLDER_INFO placeholderInfo,
            uint placeholderInfoSize);

        [DllImport("projectedfslib.dll", CharSet = CharSet.Unicode)]
        public unsafe static extern uint PrjWriteFileData(
            IntPtr namespaceVirtualizationContext,
            in Guid dataStreamId,
            byte* buffer,
            ulong byteOffset,
            uint length);


        [DllImport("projectedfslib.dll", CharSet = CharSet.Unicode)]
        public static extern void PrjStopVirtualizing(IntPtr namespaceVirtualizationContext);

        [StructLayout(LayoutKind.Sequential)]
        public struct PRJ_PLACEHOLDER_INFO
        {
            public PRJ_FILE_BASIC_INFO FileBasicInfo;
            public uint EaBufferSize;
            public uint OffsetToFirstEa;
            public uint SecurityBufferSize;
            public uint OffsetToFirstSecurityDescriptor;
            public uint StreamsInfoBufferSize;
            public uint OffsetToFirstStreamInfo;
            public PRJ_PLACEHOLDER_VERSION_INFO VersionInfo;
            public IntPtr VariableData;

            public static PRJ_PLACEHOLDER_INFO Create(PlaceholderInfo placeholderInfo)
            {
                return new PRJ_PLACEHOLDER_INFO()
                {
                    FileBasicInfo = PRJ_FILE_BASIC_INFO.Create(placeholderInfo.BasicInfo),
                    VersionInfo = placeholderInfo.VersionInfo == null ?
                        PRJ_PLACEHOLDER_VERSION_INFO.Empty :
                        PRJ_PLACEHOLDER_VERSION_INFO.Create(placeholderInfo.VersionInfo)
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PRJ_FILE_BASIC_INFO
        {
            public bool IsDirectory;
            public long FileSize;
            public long CreationTime;
            public long LastAccessTime;
            public long LastWriteTime;
            public long ChangeTime;
            public uint FileAttributes;

            public static PRJ_FILE_BASIC_INFO Create(FileBasicInfo basicInfo)
            {
                return new PRJ_FILE_BASIC_INFO
                {
                    IsDirectory = basicInfo.IsDirectory,
                    FileSize = basicInfo.FileSize,
                    CreationTime = basicInfo.CreationTime.ToFileTime(),
                    LastAccessTime = basicInfo.LastAccessTime.ToFileTime(),
                    LastWriteTime = basicInfo.LastWriteTime.ToFileTime(),
                    ChangeTime = basicInfo.ChangeTime.ToFileTime(),
                    FileAttributes = (uint)basicInfo.FileAttributes,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PRJ_PLACEHOLDER_VERSION_INFO
        {
            public const int Size = 128;
            public static readonly PRJ_PLACEHOLDER_VERSION_INFO Empty = new PRJ_PLACEHOLDER_VERSION_INFO()
            {
                ProviderId = new byte[Size],
                ContentId = new byte[Size],
            };

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Size)]
            public byte[] ProviderId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Size)]
            public byte[] ContentId;

            public static PRJ_PLACEHOLDER_VERSION_INFO Create(PlaceholderVersionInfo versionInfo)
            {
                return new PRJ_PLACEHOLDER_VERSION_INFO()
                {
                    ProviderId = versionInfo.ProviderId ?? Array.Empty<byte>(),
                    ContentId = versionInfo.ContentId ?? Array.Empty<byte>(),
                };
            }
        }

        public enum PRJ_CALLBACK_DATA_FLAGS : uint
        {
            PRJ_CB_DATA_FLAG_ENUM_RESTART_SCAN = 0x00000001,
            PRJ_CB_DATA_FLAG_ENUM_RETURN_SINGLE_ENTRY = 0x00000002
        }

        public enum PRJ_NOTIFICATION : uint
        {
            PRJ_NOTIFICATION_FILE_OPENED = 0x00000002,
            PRJ_NOTIFICATION_NEW_FILE_CREATED = 0x00000004,
            PRJ_NOTIFICATION_FILE_OVERWRITTEN = 0x00000008,
            PRJ_NOTIFICATION_PRE_DELETE = 0x00000010,
            PRJ_NOTIFICATION_PRE_RENAME = 0x00000020,
            PRJ_NOTIFICATION_PRE_SET_HARDLINK = 0x00000040,
            PRJ_NOTIFICATION_FILE_RENAMED = 0x00000080,
            PRJ_NOTIFICATION_HARDLINK_CREATED = 0x00000100,
            PRJ_NOTIFICATION_FILE_HANDLE_CLOSED_NO_MODIFICATION = 0x00000200,
            PRJ_NOTIFICATION_FILE_HANDLE_CLOSED_FILE_MODIFIED = 0x00000400,
            PRJ_NOTIFICATION_FILE_HANDLE_CLOSED_FILE_DELETED = 0x00000800,
            PRJ_NOTIFICATION_FILE_PRE_CONVERT_TO_FULL = 0x00001000,
        }

        public enum PRJ_NOTIFY_TYPES : uint
        {
            PRJ_NOTIFY_NONE = 0x00000000,
            PRJ_NOTIFY_SUPPRESS_NOTIFICATIONS = 0x00000001,
            PRJ_NOTIFY_FILE_OPENED = 0x00000002,
            PRJ_NOTIFY_NEW_FILE_CREATED = 0x00000004,
            PRJ_NOTIFY_FILE_OVERWRITTEN = 0x00000008,
            PRJ_NOTIFY_PRE_DELETE = 0x00000010,
            PRJ_NOTIFY_PRE_RENAME = 0x00000020,
            PRJ_NOTIFY_PRE_SET_HARDLINK = 0x00000040,
            PRJ_NOTIFY_FILE_RENAMED = 0x00000080,
            PRJ_NOTIFY_HARDLINK_CREATED = 0x00000100,
            PRJ_NOTIFY_FILE_HANDLE_CLOSED_NO_MODIFICATION = 0x00000200,
            PRJ_NOTIFY_FILE_HANDLE_CLOSED_FILE_MODIFIED = 0x00000400,
            PRJ_NOTIFY_FILE_HANDLE_CLOSED_FILE_DELETED = 0x00000800,
            PRJ_NOTIFY_FILE_PRE_CONVERT_TO_FULL = 0x00001000,
            PRJ_NOTIFY_USE_EXISTING_MASK = 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PRJ_CALLBACK_DATA
        {
            public uint Size;
            public PRJ_CALLBACK_DATA_FLAGS Flags;
            public IntPtr NamespaceVirtualizationContext;
            public int CommandId;
            public Guid FileId;
            public Guid DataStreamId;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string FilePathName;
            public IntPtr VersionInfo;
            public uint TriggeringProcessId;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TriggeringProcessImageFileName;
            public IntPtr InstanceContext;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct PRJ_NOTIFICATION_PARAMETERS
        {
            [FieldOffset(0)]
            public PRJ_NOTIFY_TYPES PostCreate;
            [FieldOffset(0)]
            public PRJ_NOTIFY_TYPES FileRenamed;
            [FieldOffset(0)]
            public bool FileDeletedOnHandleClose;
        }

        public delegate uint PRJ_START_DIRECTORY_ENUMERATION_CB(in PRJ_CALLBACK_DATA callbackData, in Guid enumerationId);
        public delegate uint PRJ_END_DIRECTORY_ENUMERATION_CB(in PRJ_CALLBACK_DATA callbackData, in Guid enumerationId);
        public delegate uint PRJ_GET_DIRECTORY_ENUMERATION_CB(in PRJ_CALLBACK_DATA callbackData, in Guid enumerationId, IntPtr searchExpression, IntPtr dirEntryBufferHandle);
        public delegate uint PRJ_GET_PLACEHOLDER_INFO_CB(in PRJ_CALLBACK_DATA callbackData);
        public delegate uint PRJ_GET_FILE_DATA_CB(in PRJ_CALLBACK_DATA callbackData, ulong byteOffset, uint length);
        public delegate uint PRJ_QUERY_FILE_NAME_CB(in PRJ_CALLBACK_DATA callbackData);
        public delegate uint PRJ_NOTIFICATION_CB(in PRJ_CALLBACK_DATA callbackData, bool isDirectory, PRJ_NOTIFICATION notification, string destinationFileName, in PRJ_NOTIFICATION_PARAMETERS notificationParameters);
        public delegate uint PRJ_CANCEL_COMMAND_CB(in PRJ_CALLBACK_DATA callbackData);

        public interface INativeProjectionCallbacks
        {
            uint StartDirectoryEnumerationCallback(in PRJ_CALLBACK_DATA callbackData, in Guid enumerationId);
            uint EndDirectoryEnumerationCallback(in PRJ_CALLBACK_DATA callbackData, in Guid enumerationId);
            uint GetDirectoryEnumerationCallback(in PRJ_CALLBACK_DATA callbackData, in Guid enumerationId, IntPtr searchExpression, IntPtr dirEntryBufferHandle);
            uint GetPlaceholderInfoCallback(in PRJ_CALLBACK_DATA callbackData);
            uint GetFileDataCallback(in PRJ_CALLBACK_DATA callbackData, ulong byteOffset, uint length);
        }

        public struct PRJ_CALLBACKS
        {
            public IntPtr StartDirectoryEnumerationCallback;
            public IntPtr EndDirectoryEnumerationCallback;
            public IntPtr GetDirectoryEnumerationCallback;
            public IntPtr GetPlaceholderInfoCallback;
            public IntPtr GetFileDataCallback;
            public IntPtr QueryFileNameCallback;
            public IntPtr NotificationCallback;
            public IntPtr CancelCommandCallback;

            public static PRJ_CALLBACKS Create(INativeProjectionCallbacks callbacks)
            {
                return new PRJ_CALLBACKS
                {
                    StartDirectoryEnumerationCallback = Marshal.GetFunctionPointerForDelegate<PRJ_START_DIRECTORY_ENUMERATION_CB>(callbacks.StartDirectoryEnumerationCallback),
                    EndDirectoryEnumerationCallback = Marshal.GetFunctionPointerForDelegate<PRJ_END_DIRECTORY_ENUMERATION_CB>(callbacks.EndDirectoryEnumerationCallback),
                    GetDirectoryEnumerationCallback = Marshal.GetFunctionPointerForDelegate<PRJ_GET_DIRECTORY_ENUMERATION_CB>(callbacks.GetDirectoryEnumerationCallback),
                    GetPlaceholderInfoCallback = Marshal.GetFunctionPointerForDelegate<PRJ_GET_PLACEHOLDER_INFO_CB>(callbacks.GetPlaceholderInfoCallback),
                    GetFileDataCallback = Marshal.GetFunctionPointerForDelegate<PRJ_GET_FILE_DATA_CB>(callbacks.GetFileDataCallback),
                    QueryFileNameCallback = IntPtr.Zero,
                    NotificationCallback = IntPtr.Zero,
                    CancelCommandCallback = IntPtr.Zero,
                };
            }
        }
    }
}
