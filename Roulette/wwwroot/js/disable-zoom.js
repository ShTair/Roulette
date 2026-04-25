(function () {
  let lastValidTouchEnd = 0;
  let lastValidX = null;
  let lastValidY = null;
  const DOUBLE_TAP_THRESHOLD = 500; // iOS detects double tap up to around 500ms
  const DOUBLE_TAP_DISTANCE = 50; // pixels; taps farther apart than this are not considered a double tap
  document.addEventListener('touchend', function (e) {
    const now = Date.now();
    const touch = e.changedTouches && e.changedTouches[0];
    const x = touch ? touch.clientX : null;
    const y = touch ? touch.clientY : null;
    const dt = now - lastValidTouchEnd;
    let farEnough = false; // assume same position if coordinates are unavailable
    if (x !== null && y !== null && lastValidX !== null && lastValidY !== null) {
      farEnough = Math.abs(x - lastValidX) >= DOUBLE_TAP_DISTANCE || Math.abs(y - lastValidY) >= DOUBLE_TAP_DISTANCE;
    }
    if (dt <= DOUBLE_TAP_THRESHOLD && !farEnough) {
      e.preventDefault();
    } else {
      lastValidTouchEnd = now;
      lastValidX = x;
      lastValidY = y;
    }
  }, { passive: false });

  document.addEventListener('gesturestart', function (e) {
    e.preventDefault();
  });
})();
