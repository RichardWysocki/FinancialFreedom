window.ff = window.ff || {};

/** Match MudBreakPoint.Md (960px) — used to open sidebar by default on desktop only. */
window.ffIsWideLayout = function () {
    return window.matchMedia('(min-width: 960px)').matches;
};

/**
 * MudBlazor.min.js exposes window.mudThemeProvider.watchDarkMode / stopWatchingDarkMode.
 * Some MudBlazor.dll builds invoke JS as global identifiers watchDarkThemeMedia / stopWatchingDarkThemeMedia
 * (Blazor looks these up on window, NOT on mudThemeProvider). Define globals that delegate to mudThemeProvider.
 */
(function () {
    function bridge() {
        var mtp = window.mudThemeProvider;
        if (!mtp || typeof mtp.watchDarkMode !== 'function') return;

        function watch(dotNetRef) {
            return mtp.watchDarkMode(dotNetRef);
        }

        function stop() {
            return mtp.stopWatchingDarkMode();
        }

        if (typeof window.watchDarkThemeMedia !== 'function')
            window.watchDarkThemeMedia = watch;

        if (typeof window.stopWatchingDarkThemeMedia !== 'function')
            window.stopWatchingDarkThemeMedia = stop;

        if (typeof mtp.watchDarkThemeMedia !== 'function')
            mtp.watchDarkThemeMedia = watch;

        if (typeof mtp.stopWatchingDarkThemeMedia !== 'function')
            mtp.stopWatchingDarkThemeMedia = stop;
    }

    bridge();
})();

window.ff.download = function (fileName, base64) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:application/octet-stream;base64,' + base64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

/**
 * Select entire field value when the user tabs or clicks into a Mud input (caret no longer jumps before $ / % adorners).
 * Add class ff-input-select-all on MudNumericField / MudTextField roots.
 */
(function () {
    if (window.ff.__inputSelectAllInit) return;
    window.ff.__inputSelectAllInit = true;

    function selectAll(inp) {
        window.requestAnimationFrame(function () {
            window.requestAnimationFrame(function () {
                try {
                    if (typeof inp.select === 'function') inp.select();
                    if (
                        typeof inp.setSelectionRange === 'function' &&
                        typeof inp.value === 'string'
                    )
                        inp.setSelectionRange(0, inp.value.length);
                } catch (e) {
                    /* noop */
                }
            });
        });
    }

    document.addEventListener(
        'focusin',
        function (ev) {
            var t = ev.target;
            if (!t || t.tagName !== 'INPUT') return;
            if (typeof t.closest !== 'function' || !t.closest('.ff-input-select-all')) return;
            selectAll(t);
        },
        true
    );

    /** Click-focus: Mud sometimes sets caret position after focus — re-select after pointer lifts. */
    document.addEventListener(
        'pointerup',
        function (ev) {
            var t = ev.target;
            if (!t || t.tagName !== 'INPUT') return;
            if (typeof t.closest !== 'function' || !t.closest('.ff-input-select-all')) return;
            selectAll(t);
        },
        true
    );
})();
