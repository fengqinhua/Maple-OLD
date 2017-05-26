using System.IO;

namespace Maple.Web.Environment.Extensions.Compilers {
    public interface IProjectFileParser {
        ProjectFileDescriptor Parse(string virtualPath);
        ProjectFileDescriptor Parse(Stream stream);
    }
}