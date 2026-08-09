using System;
using System.Collections;

namespace NukeLib.Utils;

public static class ExecutionUtils {
    public static void RunNextFrame(Action action) {
        Plugin.Instance?.StartCoroutine(RunNextFrameRoutine(action));
    }

    private static IEnumerator RunNextFrameRoutine(Action action) {
        yield return null;
        action?.Invoke();
    }

    public static void RunAfterFrames(int frameCount, Action action) {
        Plugin.Instance?.StartCoroutine(RunAfterFramesRoutine(frameCount, action));
    }

    private static IEnumerator RunAfterFramesRoutine(int frameCount, Action action) {
        for (int i = 0; i < frameCount; i++) {
            yield return null;
        }

        action?.Invoke();
    }
}
