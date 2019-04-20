using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Projecticate
{
    public abstract class ProjectedFileSystem : IDisposable
    {
        private readonly string _rootPath;
        private readonly Guid _instanceId;
        private readonly CallbackRouter _router;
        private IntPtr _context;

        protected ProjectedFileSystem(string rootPath)
        {
            _rootPath = rootPath;
            _instanceId = Guid.NewGuid();
            _router = new CallbackRouter(this);
        }

        public void Start()
        {
            // TODO: We probably don't HAVE to throw here...
            if (Directory.Exists(_rootPath))
            {
                throw new InvalidOperationException("The directory already exists!");
            }
            Directory.CreateDirectory(_rootPath);

            var hr = NativeMethods.PrjMarkDirectoryAsPlaceholder(
                _rootPath,
                null,
                IntPtr.Zero,
                _instanceId);
            Marshal.ThrowExceptionForHR((int)hr);

            var callbacks = NativeMethods.PRJ_CALLBACKS.Create(_router);
            hr = NativeMethods.PrjStartVirtualizing(
                _rootPath,
                in callbacks,
                IntPtr.Zero,
                IntPtr.Zero,
                out _context);
            Marshal.ThrowExceptionForHR((int)hr);
        }

        // TODO: Async!
        public abstract IEnumerable<ProjectedDirectoryEntry> EnumerateDirectory(string relativePath, string searchPattern);

        public void Dispose()
        {
            if (_context != IntPtr.Zero)
            {
                NativeMethods.PrjStopVirtualizing(_context);
            }

            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath);
            }
        }

        private class CallbackRouter : NativeMethods.INativeProjectionCallbacks
        {
            private ProjectedFileSystem _fs;
            private Dictionary<Guid, DirectoryEnumerationState> _enumerations = new Dictionary<Guid, DirectoryEnumerationState>();

            public CallbackRouter(ProjectedFileSystem fs)
            {
                _fs = fs;
            }

            public uint StartDirectoryEnumerationCallback(in NativeMethods.PRJ_CALLBACK_DATA callbackData, in Guid enumerationId)
            {
                Console.WriteLine($"StartEnum: {callbackData.FilePathName}");
                _enumerations[enumerationId] = new DirectoryEnumerationState(callbackData.FilePathName);
                return 0;
            }

            public uint GetDirectoryEnumerationCallback(in NativeMethods.PRJ_CALLBACK_DATA callbackData, in Guid enumerationId, IntPtr searchExpression, IntPtr dirEntryBufferHandle)
            {
                if (TryGetEnumeration(enumerationId, out var state))
                {
                    // We presume that we won't get parallel callbacks for the same enumeration...
                    if (state.Enumerator == null || (callbackData.Flags & NativeMethods.PRJ_CALLBACK_DATA_FLAGS.PRJ_CB_DATA_FLAG_ENUM_RESTART_SCAN) != 0)
                    {
                        var searchExpr = Marshal.PtrToStringUni(searchExpression);
                        // Complete any existing enumeration
                        state.Enumerator?.Dispose();

                        var enumerable = _fs.EnumerateDirectory(state.FilePathName, searchExpr);
                        state.Enumerator = enumerable.GetEnumerator();
                        state.HasItem = state.Enumerator.MoveNext();
                    }

                    while (state.HasItem)
                    {
                        if (!TryWriteDirectoryEntry(dirEntryBufferHandle, state.Enumerator.Current))
                        {
                            // We ran out of space. Exit for now. We'll be called back again.
                            break;
                        }
                        state.HasItem = state.Enumerator.MoveNext();
                    }

                    // We're done for now.
                    return 0;
                }
                throw new InvalidOperationException("Unable to find active enumeration!");
            }

            public uint EndDirectoryEnumerationCallback(in NativeMethods.PRJ_CALLBACK_DATA callbackData, in Guid enumerationId)
            {
                if (TryRemoveEnumeration(enumerationId, out var state))
                {
                    state.Dispose();
                    return 0;
                }
                throw new InvalidOperationException("Unable to find active enumeration!");
            }

            public uint GetFileDataCallback(in NativeMethods.PRJ_CALLBACK_DATA callbackData, ulong byteOffset, uint length)
            {
                // TODO
                Console.WriteLine($"GetFileData: {callbackData.FilePathName}");
                return 0;
            }

            public uint GetPlaceholderInfoCallback(in NativeMethods.PRJ_CALLBACK_DATA callbackData)
            {
                // TODO
                Console.WriteLine($"GetPlaceholderInfo: {callbackData.FilePathName}");
                return 0;
            }

            private bool TryRemoveEnumeration(Guid enumerationId, out DirectoryEnumerationState state)
            {
                lock (_enumerations)
                {
                    return _enumerations.Remove(enumerationId, out state);
                }
            }

            private bool TryGetEnumeration(Guid enumerationId, out DirectoryEnumerationState state)
            {
                lock (_enumerations)
                {
                    return _enumerations.TryGetValue(enumerationId, out state);
                }
            }

            private bool TryWriteDirectoryEntry(IntPtr dirEntryBufferHandle, ProjectedDirectoryEntry entry)
            {
                var basicInfo = NativeMethods.PRJ_FILE_BASIC_INFO.Create(entry.BasicInfo);
                var hr = NativeMethods.PrjFillDirEntryBuffer(
                    entry.FileName,
                    in basicInfo,
                    dirEntryBufferHandle);
                if (hr == NativeMethods.HRESULT_ERROR_INSUFFICIENT_BUFFER)
                {
                    return false;
                }
                Marshal.ThrowExceptionForHR((int)hr);
                return true;
                throw new NotImplementedException();
            }

            private class DirectoryEnumerationState : IDisposable
            {
                public string FilePathName { get; }
                public bool HasItem { get; set; }
                public IEnumerator<ProjectedDirectoryEntry> Enumerator { get; set; }

                public DirectoryEnumerationState(string filePathName)
                {
                    FilePathName = filePathName;
                }

                public void Dispose()
                {
                    Enumerator?.Dispose();
                }
            }
        }
    }
}
