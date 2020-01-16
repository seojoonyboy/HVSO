using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationProgress : DownloadProgress {
    public override void StartProgress() {
        base.StartProgress();
    }

    public override void OnFinished() {
        base.OnFinished();
    }

    public override void OnProgress(long downloaded, long downloadLength) {
        base.OnProgress(downloaded, downloadLength);
    }
}
