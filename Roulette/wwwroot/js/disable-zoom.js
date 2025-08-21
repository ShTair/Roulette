(function () {
  let lastTouchEnd = 0;
  const DOUBLE_TAP_THRESHOLD = 500; // iOS detects double tap up to around 500ms
  document.addEventListener('touchend', function (e) {
    const now = Date.now();
    if (now - lastTouchEnd <= DOUBLE_TAP_THRESHOLD) {
      e.preventDefault();
    }
    lastTouchEnd = now;
  }, { passive: false });

  document.addEventListener('gesturestart', function (e) {
    e.preventDefault();
  });
})();
