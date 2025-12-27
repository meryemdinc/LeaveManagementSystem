// ==========================================
// YARDIMCI FONKSİYON: Giriş Başarılı Olunca
// ==========================================
function handleLoginSuccess(data) {
    // 1. Token'dan ROL bilgisini Çözümle (Decode)
    const payloadBase64 = data.token.split('.')[1];
    const decodedJson = atob(payloadBase64);
    const payload = JSON.parse(decodedJson);

    // Rol claim'ini bul (Bazen uzun URL, bazen 'role' olarak gelir)
    const role = payload["role"] || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["uid"];

    // 2. Tarayıcı Hafızasına Kaydet
    localStorage.setItem("token", data.token);
    localStorage.setItem("userEmail", data.email);
    localStorage.setItem("userId", data.id);
    localStorage.setItem("userRole", role || "Employee");

    // 3. Başarı Mesajı ve Yönlendirme
    Swal.fire({
        icon: 'success',
        title: 'Hoşgeldiniz!',
        text: 'Yönlendiriliyorsunuz...',
        timer: 1500,
        showConfirmButton: false
    }).then(() => {
        // dashboard.html'in de wwwroot içinde olduğundan emin ol
        window.location.href = "dashboard.html";
    });
}

// ==========================================
// 1. STANDART GİRİŞ (Email & Şifre)
// ==========================================
document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    Swal.fire({ title: 'Giriş Yapılıyor...', didOpen: () => { Swal.showLoading() } });

    const loginData = { email: email, password: password };

    try {
        // DÜZELTME: API_BASE_URL yerine direkt '/api' kullanıyoruz.
        const response = await fetch('/api/Auth/login', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(loginData)
        });

        if (response.ok) {
            const data = await response.json();
            handleLoginSuccess(data);
        } else {
            Swal.fire({ icon: 'error', title: 'Giriş Başarısız', text: 'E-posta veya şifre hatalı!' });
        }
    } catch (error) {
        console.error(error);
        Swal.fire({ icon: 'error', title: 'Bağlantı Hatası', text: 'Sunucuya ulaşılamadı.' });
    }
});

// ==========================================
// 2. GOOGLE GİRİŞ (GERÇEKÇİ SİMÜLASYON 🎭)
// ==========================================
const btnGoogle = document.getElementById("btnGoogleLogin");

if (btnGoogle) {
    btnGoogle.addEventListener("click", async () => {

        // ADIM 1: Sahte Google Penceresi Aç (SweetAlert ile HTML)
        const { value: selectedAccount } = await Swal.fire({
            title: 'Google ile Oturum Aç',
            html: `
                <div style="text-align: left; margin-bottom: 10px; font-size: 0.9rem; color: #555;">Bir hesap seçin:</div>
                <div class="list-group text-start">
                    <button class="list-group-item list-group-item-action d-flex align-items-center p-3" onclick="Swal.clickConfirm()" value="gercek">
                        <img src="https://www.gstatic.com/firebasejs/ui/2.0.0/images/auth/google.svg" width="24" class="me-3">
                        <div>
                            <div class="fw-bold text-dark">Meryem Dinç</div>
                            <small class="text-muted">meryemdinc45@gmail.com</small>
                        </div>
                    </button>
                    <button class="list-group-item list-group-item-action d-flex align-items-center p-3" onclick="Swal.clickConfirm()" value="demo">
                        <img src="https://www.gstatic.com/firebasejs/ui/2.0.0/images/auth/google.svg" width="24" class="me-3">
                        <div>
                            <div class="fw-bold text-dark">Demo Kullanıcı</div>
                            <small class="text-muted">demo_user@nova.com</small>
                        </div>
                    </button>
                </div>
                <div class="mt-3 text-muted" style="font-size: 0.8rem;">
                    Uygulamaya gitmek için Google, adınızı, e-posta adresinizi ve profil resminizi NovaLeave ile paylaşacaktır.
                </div>
            `,
            showConfirmButton: false,
            showCloseButton: true,
            width: '450px',
            background: '#fff',
            customClass: { popup: 'rounded-4' }
        });

        // Eğer kullanıcı çarpıya basıp kapatırsa dur
        if (!selectedAccount && !Swal.getTimerLeft()) return;

        // ADIM 2: Seçim yapıldı, Backend'e bağlanılıyor süsü ver
        Swal.fire({
            title: 'Google Doğrulanıyor...',
            text: 'Lütfen bekleyin',
            timer: 1500, // 1.5 saniye bekle (Gerçekçilik için)
            timerProgressBar: true,
            didOpen: () => { Swal.showLoading() }
        }).then(async () => {

            // ADIM 3: Backend'e MOCK Token gönder (Gerçek işlem)
            try {
                // DÜZELTME: API_BASE_URL yerine direkt '/api' kullanıyoruz.
                const response = await fetch('/api/Auth/google-login', {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        provider: "Google",
                        idToken: "MOCK_GOOGLE_TOKEN_12345" // Bizim gizli anahtar
                    })
                });

                if (response.ok) {
                    const data = await response.json();
                    handleLoginSuccess(data); // Ortak fonksiyonu çağır
                } else {
                    Swal.fire('Hata', 'Google doğrulaması başarısız.', 'error');
                }
            } catch (error) {
                console.error(error);
                Swal.fire('Hata', 'Sunucu hatası.', 'error');
            }
        });
    });
}