// 1. Güvenlik Kontrolü (Token yoksa Login'e at)
const token = localStorage.getItem("token");
const role = localStorage.getItem("userRole");
const email = localStorage.getItem("userEmail");

if (!token) {
    window.location.href = "index.html";
}

// 2. Kullanıcı Bilgisi ve Menü Ayarı
document.getElementById("userInfo").textContent = `${email} (${role})`;

if (role === "Admin") {
    document.getElementById("adminMenu").classList.remove("d-none"); // Admin menüsünü aç
}

// Sayfa Yüklendiğinde
document.addEventListener("DOMContentLoaded", () => {
    loadLeaveRequests();
});

// 3. Verileri Çekme Fonksiyonu
async function loadLeaveRequests() {
    const tableBody = document.getElementById("leaveTableBody");
    
    try {
        // Admin ise HEPSİNİ, Değilse KENDİNKİNİ çek
        const endpoint = role === "Admin" 
            ? `${API_BASE_URL}/LeaveRequests` 
            : `${API_BASE_URL}/LeaveRequests/MyLeaves`;

        const response = await fetch(endpoint, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (response.ok) {
            const data = await response.json();
            renderTable(data);
        } else {
            tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Veriler yüklenemedi!</td></tr>';
        }
    } catch (error) {
        console.error(error);
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Bağlantı Hatası!</td></tr>';
    }
}

// 4. Tabloyu Ekrana Basma
function renderTable(requests) {
    const tableBody = document.getElementById("leaveTableBody");
    tableBody.innerHTML = ""; // Temizle

    if (requests.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Henüz hiç izin talebi yok.</td></tr>';
        return;
    }

    requests.forEach(req => {
        // Durum Rozetleri
        let statusBadge = '<span class="badge bg-warning text-dark">Bekliyor</span>';
        if (req.status === 2) statusBadge = '<span class="badge bg-success">Onaylandı</span>';
        if (req.status === 3) statusBadge = '<span class="badge bg-danger">Reddedildi</span>';

        // İşlemler (Sadece Admin ve Bekleyen ise Buton Göster)
        let actions = '<span class="text-muted small">-</span>';
        if (role === "Admin" && req.status === 1) { // 1 = Pending
            actions = `
                <button class="btn btn-sm btn-success me-1" onclick="approveRequest(${req.id}, true)">✔</button>
                <button class="btn btn-sm btn-danger" onclick="approveRequest(${req.id}, false)">✖</button>
            `;
        }

        const row = `
            <tr>
                <td class="fw-bold">${req.employee ? req.employee.firstName + ' ' + req.employee.lastName : 'Ben'}</td>
                <td>${req.leaveType}</td>
                <td>${new Date(req.startDate).toLocaleDateString()} ${new Date(req.startDate).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</td>
                <td>${new Date(req.endDate).toLocaleDateString()} ${new Date(req.endDate).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</td>
                <td>${statusBadge}</td>
                <td class="text-end pe-3">${actions}</td>
            </tr>
        `;
        tableBody.innerHTML += row;
    });
}

// 5. Yeni Talep Gönderme
document.getElementById("createLeaveForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const payload = {
        leaveTypeId: parseInt(document.getElementById("leaveTypeId").value),
        startDate: new Date(document.getElementById("startDate").value).toISOString(),
        endDate: new Date(document.getElementById("endDate").value).toISOString(),
        reason: document.getElementById("reason").value,
        employeeId: 0 // Backend halledecek
    };

    try {
        const response = await fetch(`${API_BASE_URL}/LeaveRequests`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            // Modalı Kapat
            const modalEl = document.getElementById('createLeaveModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();

            Swal.fire('Başarılı', 'Talebiniz oluşturuldu.', 'success');
          
        } else {
            const errorData = await response.json();
            
            // Hata mesajı string mi yoksa obje mi kontrol et
            let errorMessage = "İşlem başarısız.";
            if (errorData.message) errorMessage = errorData.message;
            else if (errorData.errors) errorMessage = JSON.stringify(errorData.errors);
            
            Swal.fire('Hata', errorMessage, 'error');
        }
    } catch (error) {
        console.error(error);
    }
});

// 6. Çıkış Yapma
document.getElementById("btnLogout").addEventListener("click", () => {
    localStorage.clear();
    window.location.href = "index.html";
});

// Sidebar Toggle
document.getElementById("menu-toggle").addEventListener("click", function (e) {
    e.preventDefault();
    document.body.classList.toggle("sb-sidenav-toggled");
});