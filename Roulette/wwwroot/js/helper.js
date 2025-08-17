(function () {
    function tryVibrate(ms) {
        try {
            if (navigator?.vibrate) {
                navigator.vibrate(ms);
            }
        } catch { }
    }
    let canvas, ctx, items = [], angle = 0, angleMap = [];
    let spinning = false;
    let idle = false;
    const idleSpeed = 0.2; // slow rotation when idle
    let speed = 0; // angular velocity in radians per second
    let animationId = null; // requestAnimationFrame id
    let lastTime = null; // timestamp of previous frame for consistent timing
    let autoStopTimeout = null;
    let autoStopEnabled = true;
    let startSpeedSetting = 18;
    let autoStopMinMs = 2000;
    let stopDurationMs = 2000;
    let borderColor = '#808080';
    let stopStartTime = null;
    let stopInitialSpeed = 0;
    let dotNetHelper = null;
    let dpr = 1;
    let startX = null, startY = null, startId = null, startTime = 0;

    function getWeight(item) {
        const w = Number(item?.weight);
        return isFinite(w) && w > 0 ? w : 1;
    }

    function computeAngles() {
        angleMap = [];
        const total = items.reduce((s, it) => s + getWeight(it), 0);
        let cur = 0;
        for (let i = 0; i < items.length; i++) {
            cur += getWeight(items[i]) / total * 2 * Math.PI;
            angleMap.push(cur);
        }
    }

    function getContrastColor(hex) {
        if (!hex) return 'black';
        hex = String(hex).replace(/^#/, '');
        if (hex.length === 3) {
            hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
        }
        if (hex.length !== 6) return 'black';
        const r = parseInt(hex.substring(0, 2), 16);
        const g = parseInt(hex.substring(2, 4), 16);
        const b = parseInt(hex.substring(4, 6), 16);
        const brightness = (r * 299 + g * 587 + b * 114) / 1000;
        return brightness > 128 ? 'black' : 'white';
    }

    function drawSlice(start, end, item, index, radius, textMid) {
        ctx.beginPath();
        ctx.moveTo(0, 0);
        ctx.arc(0, 0, radius, start, end);
        const color = item?.color || (index % 2 === 0 ? '#f8a' : '#8af');
        ctx.fillStyle = color;
        ctx.fill();
        ctx.stroke();

        ctx.save();
        ctx.rotate((start + end) / 2);
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillStyle = getContrastColor(color);
        const text = item?.text || item;
        const baseSize = 16;
        const maxWidth = 2 * textMid * 0.7;
        ctx.font = `${baseSize}px 'BIZ UDPGothic', sans-serif`;
        const metrics = ctx.measureText(text);
        let fontSize = baseSize;
        if (metrics.width > maxWidth) {
            fontSize = Math.max(6, Math.floor(baseSize * maxWidth / metrics.width));
            ctx.font = `${fontSize}px 'BIZ UDPGothic', sans-serif`;
        }
        ctx.fillText(text, textMid, 0);
        ctx.restore();
    }

    function drawCenter(radius, lineWidth, ratio) {
        ctx.beginPath();
        ctx.lineWidth = lineWidth;
        ctx.fillStyle = 'white';
        ctx.arc(0, 0, radius * ratio, 0, 2 * Math.PI);
        ctx.fill();
        ctx.strokeStyle = borderColor;
        ctx.stroke();

        ctx.beginPath();
        ctx.arc(0, 0, radius, 0, 2 * Math.PI);
        ctx.strokeStyle = borderColor;
        ctx.stroke();
    }

    function draw() {
        if (!ctx) return;

        const width = canvas.width / dpr;
        const height = canvas.height / dpr;
        const radius = Math.min(width, height) / 2 - 5;

        const lineWidth = 3;
        const centerRatio = 0.1;
        const textMid = (radius - lineWidth / 2) / (1 - 0.12) / 2;

        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.save();
        ctx.translate(width / 2, height / 2);
        ctx.rotate(angle);
        ctx.strokeStyle = borderColor;

        const total = items.reduce((s, it) => s + getWeight(it), 0);
        let current = 0;
        for (let i = 0; i < items.length; i++) {
            const slice = getWeight(items[i]) / total * 2 * Math.PI;
            drawSlice(current, current + slice, items[i], i, radius, textMid);
            current += slice;
        }

        drawCenter(radius, lineWidth, centerRatio);
        ctx.restore();
    }

    function tick(timestamp) {
        if (lastTime === null) lastTime = timestamp;
        const delta = (timestamp - lastTime) / 1000; // seconds since last frame
        lastTime = timestamp;

        if (spinning) {
            angle += speed * delta;
        } else if (speed > 0) {
            angle += speed * delta;
            if (stopStartTime === null) {
                stopStartTime = timestamp;
                stopInitialSpeed = speed;
            }
            const progress = (timestamp - stopStartTime) / stopDurationMs;
            if (progress >= 1) {
                speed = 0;
                draw();
                tryVibrate(50);
                if (dotNetHelper) {
                    const result = getCurrentIndex();
                    dotNetHelper.invokeMethodAsync('OnRouletteStopped', result);
                }
                lastTime = null;
                stopStartTime = null;
                animationId = null;
                return;
            } else {
                const eased = Math.pow(1 - progress, 2);
                speed = stopInitialSpeed * eased;
            }
        } else if (idle) {
            angle += idleSpeed * delta;
        } else {
            draw();
            animationId = null;
            return;
        }
        draw();
        animationId = requestAnimationFrame(tick);
    }

    function getCurrentIndex() {
        const n = items.length;
        if (n === 0) return 0;
        const pointerAngle = -Math.PI / 2;
        let orientation = (pointerAngle - angle) % (2 * Math.PI);
        if (orientation < 0) orientation += 2 * Math.PI;
        for (let i = 0; i < angleMap.length; i++) {
            if (orientation < angleMap[i]) return i;
        }
        return n - 1;
    }

    window.rouletteHelper = window.rouletteHelper || {};

    function ensureTick() {
        if (animationId === null) {
            lastTime = null;
            animationId = requestAnimationFrame(tick);
        }
    }

    function removeSwipeHandlers() {
        if (!canvas) return;
        canvas.removeEventListener('pointerdown', onPointerDown);
        canvas.removeEventListener('pointerup', onPointerUp);
    }

    function onPointerDown(e) {
        startX = e.clientX;
        startY = e.clientY;
        startId = e.pointerId;
        startTime = e.timeStamp;
    }

    function onPointerUp(e) {
        if (startId !== e.pointerId) return;
        const dx = e.clientX - startX;
        const dy = e.clientY - startY;
        const distance = Math.hypot(dx, dy);
        const dt = e.timeStamp - startTime;
        startId = null;
        if (distance > 30 && dt < 500) {
            canvas.dispatchEvent(new Event('click', { bubbles: true }));
        }
    }

    function addSwipeHandlers() {
        if (!canvas) return;
        canvas.addEventListener('pointerdown', onPointerDown);
        canvas.addEventListener('pointerup', onPointerUp);
    }

    window.rouletteHelper.initialize = function (id, itemsArr, dotNetRef, idleSpin = true) {
        removeSwipeHandlers();
        canvas = document.getElementById(id);
        if (!canvas) return;
        ctx = canvas.getContext('2d');

        dpr = window.devicePixelRatio || 1;
        const displayWidth = canvas.clientWidth;
        const displayHeight = canvas.clientHeight;
        if (canvas.width !== displayWidth * dpr || canvas.height !== displayHeight * dpr) {
            canvas.width = displayWidth * dpr;
            canvas.height = displayHeight * dpr;
            canvas.style.width = displayWidth + 'px';
            canvas.style.height = displayHeight + 'px';
        }
        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

        items = Array.isArray(itemsArr) ? itemsArr : Array.from(itemsArr || []);
        dotNetHelper = dotNetRef || null;
        computeAngles();
        draw();
        addSwipeHandlers();
        idle = !!idleSpin;
        if (idle) ensureTick();
    };

    window.rouletteHelper.setItems = function (itemsArr) {
        items = Array.isArray(itemsArr) ? itemsArr : Array.from(itemsArr || []);
        computeAngles();
        if (!spinning) {
            draw();
        }
        if (idle) ensureTick();
    };

    window.rouletteHelper.setAutoStopEnabled = function (value) {
        autoStopEnabled = !!value;
        if (!autoStopEnabled && autoStopTimeout) {
            clearTimeout(autoStopTimeout);
            autoStopTimeout = null;
        }
    };

    window.rouletteHelper.applySettings = function (settings) {
        if (!settings) return;
        if (typeof settings.startSpeed === 'number') startSpeedSetting = settings.startSpeed;
        if (typeof settings.autoStopDelayMinSeconds === 'number') autoStopMinMs = settings.autoStopDelayMinSeconds * 1000;
        if (typeof settings.stopDurationSeconds === 'number') stopDurationMs = settings.stopDurationSeconds * 1000;
        if (typeof settings.borderColor === 'string') borderColor = settings.borderColor;
        if (typeof settings.idleSpin === 'boolean') idle = settings.idleSpin;
        if (!spinning) draw();
        if (idle) ensureTick();
    };

    window.rouletteHelper.toggleSpin = function () {
        if (spinning) {
            spinning = false;
            idle = false;
            tryVibrate(50);
            if (autoStopTimeout) {
                clearTimeout(autoStopTimeout);
                autoStopTimeout = null;
            }
        } else {
            spinning = true;
            idle = false;
            speed = startSpeedSetting;
            stopStartTime = null;
            lastTime = null;
            ensureTick();
            tryVibrate(50);
            if (autoStopTimeout) {
                clearTimeout(autoStopTimeout);
            }
            if (autoStopEnabled) {
                const rotationMs = startSpeedSetting > 0 ? 2 * Math.PI / startSpeedSetting * 1000 : 0;
                const delay = autoStopMinMs + Math.random() * rotationMs;
                autoStopTimeout = setTimeout(function () {
                    if (spinning) {
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnAutoStop');
                        }
                        spinning = false;
                        idle = false;
                        tryVibrate(50);
                    }
                }, delay);
            }
        }
    };

    window.rouletteHelper.dispose = function () {
        spinning = false;
        idle = false;
        if (animationId !== null) {
            cancelAnimationFrame(animationId);
            animationId = null;
        }
        if (autoStopTimeout) {
            clearTimeout(autoStopTimeout);
            autoStopTimeout = null;
        }
        removeSwipeHandlers();
        canvas = null;
        ctx = null;
        items = [];
        dotNetHelper = null;
    };

    window.rouletteHelper.getContainerCenter = function (id) {
        try {
            const el = document.getElementById(id);
            if (!el) return null;
            const rect = el.getBoundingClientRect();
            return [rect.left + rect.width / 2, rect.top + rect.height / 2];
        } catch { return null; }
    };


    window.downloadFile = function (fileName, content) {
        try {
            const blob = new Blob([content], { type: 'text/plain' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            a.style.display = 'none';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        } catch { }
    };

    window.triggerInputFile = function (element) {
        try {
            element?.click();
        } catch { }
    };
})();
