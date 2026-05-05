/**
 * MudBlazor 9.x: observeMainContainer() can throw ReferenceError — it logs using
 * `containerClass`, which is never defined when the popover main container is missing yet.
 * That aborts WASM startup and breaks all popovers / selects.
 *
 * We wrap the original: retry briefly while WASM paints, then swallow only that known bug.
 */
(function () {
    var mp = window.mudPopover;
    if (!mp || !mp.constructor || !mp.constructor.prototype) return;

    var proto = mp.constructor.prototype;
    var orig = proto.observeMainContainer;
    if (typeof orig !== "function") return;

    var maxRetries = 120;

    proto.observeMainContainer = function () {
        var self = this;
        try {
            orig.call(self);
            self._ffPopoverObsRetry = 0;
            return;
        } catch (e) {
            var msg = e && e.message ? String(e.message) : "";
            var isContainerClassBug =
                e instanceof ReferenceError &&
                msg.indexOf("containerClass") !== -1;

            if (!isContainerClassBug) throw e;

            var n = self._ffPopoverObsRetry | 0;
            if (n < maxRetries) {
                self._ffPopoverObsRetry = n + 1;
                requestAnimationFrame(function () {
                    proto.observeMainContainer.call(self);
                });
                return;
            }

            self._ffPopoverObsRetry = 0;
            console.warn(
                "[FinancialFreedom] MudPopover observeMainContainer: suppressed repeated ReferenceError (" +
                    msg +
                    "). Dropdowns should still work via .mud-popover-provider."
            );
        }
    };
})();
