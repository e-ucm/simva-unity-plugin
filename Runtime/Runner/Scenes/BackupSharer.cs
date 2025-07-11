using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simva
{
    public class BackupSharer : MonoBehaviour
    {
        public void Share()
        {
            string traces = "";
            string filename = "";

            try
            {
                filename = Xasu.XasuTracker.Instance.TrackerConfig.BackupFileName;
                traces = File.ReadAllText(Application.temporaryCachePath + Xasu.XasuTracker.Instance.TrackerConfig.BackupFileName);
            }
            catch (System.Exception ex){ SimvaPlugin.Instance.Log("Couldn't read traces: " + ex.Message); }

            if (string.IsNullOrEmpty(traces))
            {
                DirectoryInfo info = new DirectoryInfo(Application.temporaryCachePath);
                FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).Reverse().ToArray();
                foreach (FileInfo file in files)
                {
                    traces = File.ReadAllText(file.FullName);
                    filename = file.Name;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(traces))
            {
                string filePath = Path.Combine(Application.temporaryCachePath, filename);
                File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(traces));

                new NativeShare().AddFile(filePath)
                    .SetSubject(SimvaPlugin.Instance.GetName("BackupOfMsg") + filename).SetText(SimvaPlugin.Instance.GetName("BackupJoinedMsg"))
                    .SetCallback((result, shareTarget) => SimvaPlugin.Instance.Log("Share result: " + result + ", selected app: " + shareTarget))
                    .Share();
            }
        }
    }
}
