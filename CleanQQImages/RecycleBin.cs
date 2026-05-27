using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace CleanQQImages;

public static class RecycleBin
{
    /// <summary>
    /// 静默将文件或文件夹删除到回收站
    /// </summary>
    /// <param name="path">文件或文件夹的绝对/相对路径</param>
    /// <returns>是否删除成功</returns>
    public static unsafe bool Send(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        var fullPath = Path.GetFullPath(path);

        // Win32 API 要求 pFrom 路径字符串必须以两个 '\0' 结尾
        fixed (char* pFrom = $"{fullPath}\0\0")
        {
            var fileOp = new SHFILEOPSTRUCTW
            {
                hwnd = default,
                wFunc = PInvoke.FO_DELETE,
                pFrom = pFrom,
                pTo = null,
                fFlags = (ushort)(
                    // 可撤销，移至回收站
                    FILEOPERATION_FLAGS.FOF_ALLOWUNDO |
                    // 静默操作
                    FILEOPERATION_FLAGS.FOF_NO_UI
                ),
            };

            return PInvoke.SHFileOperation(ref fileOp) == 0;
        }
    }
}
