(function () {
    let canvas, ctx, items = [], angle = 0, angleMap = [];
    let spinning = false;
    let speed = 0;
    let autoStopTimeout = null;
    let autoStopEnabled = true;
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

    function draw() {
        if (!ctx) return;
        const width = canvas.width / dpr;
        const height = canvas.height / dpr;
        const radius = Math.min(width, height) / 2 - 5;
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.save();
        ctx.translate(width / 2, height / 2);
        ctx.rotate(angle);
        ctx.strokeStyle = 'gray';
        const total = items.reduce((s, it) => s + getWeight(it), 0);
        let current = 0;
        for (let i = 0; i < items.length; i++) {
            const slice = getWeight(items[i]) / total * 2 * Math.PI;
            const start = current;
            const end = current + slice;
            ctx.beginPath();
            ctx.moveTo(0, 0);
            ctx.arc(0, 0, radius, start, end);
            const color = items[i]?.color || (i % 2 === 0 ? '#f8a' : '#8af');
            ctx.fillStyle = color;
            ctx.fill();
            ctx.stroke();
            ctx.save();
            const mid = (start + end) / 2;
            ctx.rotate(mid);
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = 'black';
            ctx.font = "16px 'BIZ UDPGothic', sans-serif";
            const text = items[i]?.text || items[i];
            ctx.fillText(text, radius * 0.6, 0);
            ctx.restore();
            current += slice;
        }
        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.fillStyle = 'white';
        ctx.arc(0, 0, radius * 0.1, 0, 2 * Math.PI);
        ctx.fill();
        ctx.stroke();

        ctx.beginPath();
        ctx.arc(0, 0, radius, 0, 2 * Math.PI);
        ctx.stroke();
        ctx.restore();
    }

    function tick() {
        angle += speed;
        if (!spinning) {
            speed *= 0.97;
            if (speed < 0.001) {
                speed = 0;
                draw();
                if (dotNetHelper) {
                    const result = getCurrentIndex();
                    dotNetHelper.invokeMethodAsync('OnRouletteStopped', result);
                }
                return;
            }
        }
        draw();
        requestAnimationFrame(tick);
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

    window.rouletteHelper.initialize = function (id, itemsArr, dotNetRef) {
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
    };

    window.rouletteHelper.setItems = function (itemsArr) {
        items = Array.isArray(itemsArr) ? itemsArr : Array.from(itemsArr || []);
        computeAngles();
        if (!spinning) {
            draw();
        }
    };

    window.rouletteHelper.setAutoStopEnabled = function (value) {
        autoStopEnabled = !!value;
        if (!autoStopEnabled && autoStopTimeout) {
            clearTimeout(autoStopTimeout);
            autoStopTimeout = null;
        }
    };

    window.rouletteHelper.toggleSpin = function () {
        if (spinning) {
            spinning = false;
            if (autoStopTimeout) {
                clearTimeout(autoStopTimeout);
                autoStopTimeout = null;
            }
        } else {
            spinning = true;
            speed = 0.3;
            requestAnimationFrame(tick);
            if (autoStopTimeout) {
                clearTimeout(autoStopTimeout);
            }
            if (autoStopEnabled) {
                autoStopTimeout = setTimeout(function () {
                    if (spinning) {
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnAutoStop');
                        }
                        spinning = false;
                    }
                }, 2000 + Math.random() * 1000);
            }
        }
    };
})();
