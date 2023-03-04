using System.IO;
using System.Threading.Tasks;

namespace uGuardian.Utilities {
	public static class AsyncUtils {
		public static async Task<byte[]> LoadFile(FileInfo file) {
			byte[] result;
			using (var stream = file.OpenRead()) {
				result = new byte[stream.Length];
				await stream.ReadAsync(result, 0, (int)stream.Length).ConfigureAwait(false);
			}
			return result;
		}
	}
}