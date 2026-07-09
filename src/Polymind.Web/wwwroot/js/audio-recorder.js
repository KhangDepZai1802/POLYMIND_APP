// Ghi âm tin nhắn thoại bằng MediaRecorder. Trả dữ liệu về .NET qua IJSStreamReference
// (tránh giới hạn kích thước message của SignalR trên Blazor Server).
let mediaRecorder = null;
let chunks = [];
let stream = null;
let lastBlob = null;
let lastSize = 0;
let startedAt = 0;

export async function start() {
    chunks = [];
    lastBlob = null;
    lastSize = 0;
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        throw new Error('Trình duyệt không hỗ trợ ghi âm, hoặc trang không mở qua HTTPS/localhost nên bị chặn micro.');
    }
    try {
        stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    } catch (e) {
        const name = (e && e.name) ? e.name : '';
        if (name === 'NotAllowedError' || name === 'SecurityError' || name === 'PermissionDeniedError') {
            throw new Error('Bạn đã chặn quyền micro. Bấm biểu tượng khóa 🔒 (hoặc micro) trên thanh địa chỉ → cho phép Micro → tải lại trang rồi thử lại.');
        }
        if (name === 'NotFoundError' || name === 'DevicesNotFoundError') {
            throw new Error('Không tìm thấy thiết bị micro nào trên máy.');
        }
        if (name === 'NotReadableError' || name === 'TrackStartError') {
            throw new Error('Micro đang được ứng dụng khác sử dụng. Đóng ứng dụng đó rồi thử lại.');
        }
        throw new Error('Không truy cập được micro' + ((e && e.message) ? (': ' + e.message) : '.'));
    }
    let opts = undefined;
    if (window.MediaRecorder && MediaRecorder.isTypeSupported && MediaRecorder.isTypeSupported('audio/webm')) {
        opts = { mimeType: 'audio/webm' };
    }
    mediaRecorder = opts ? new MediaRecorder(stream, opts) : new MediaRecorder(stream);
    mediaRecorder.ondataavailable = e => { if (e.data && e.data.size > 0) chunks.push(e.data); };
    mediaRecorder.start();
    startedAt = Date.now();
    return true;
}

// Dừng ghi âm, gộp blob thành bytes; trả về metadata (không kèm dữ liệu nhị phân).
export async function stop() {
    return await new Promise((resolve) => {
        if (!mediaRecorder) { resolve(null); return; }
        mediaRecorder.onstop = async () => {
            const type = (mediaRecorder.mimeType || 'audio/webm').split(';')[0];
            // Giữ nguyên Blob (createJSStreamReference hỗ trợ Blob ổn định hơn Uint8Array).
            lastBlob = new Blob(chunks, { type });
            lastSize = lastBlob.size;
            if (stream) stream.getTracks().forEach(t => t.stop());
            const ext = type.includes('ogg') ? 'ogg' : (type.includes('mp4') || type.includes('mpeg') ? 'm4a' : 'webm');
            const seconds = Math.max(1, Math.round((Date.now() - startedAt) / 1000));
            resolve({ mimeType: type, ext, size: lastSize, seconds });
        };
        try { mediaRecorder.stop(); } catch { resolve(null); }
    });
}

// Trả stream nhị phân để .NET đọc (IJSStreamReference). Truyền Blob — luôn hợp lệ.
export function getAudioStream() {
    return lastBlob ? DotNet.createJSStreamReference(lastBlob) : null;
}

export function cancel() {
    try { if (mediaRecorder && mediaRecorder.state !== 'inactive') mediaRecorder.stop(); } catch { }
    if (stream) stream.getTracks().forEach(t => t.stop());
    chunks = [];
    lastBlob = null;
    lastSize = 0;
}
