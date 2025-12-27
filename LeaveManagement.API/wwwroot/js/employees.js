const token = localStorage.getItem("token");
const role = localStorage.getItem("userRole");
const email = localStorage.getItem("userEmail");

// 1. GÜVENLİK: Admin değilse bu sayfaya giremez!
if (!token || role !== "Admin") {
    alert("Bu sayfaya erişim yetkiniz yok!");
    window.location.href = "dashboard.html";
}

document.getElementById("userInfo").textContent = `${email} (Admin)`;

// Sayfa yüklenince listeyi çek
document.addEventListener("DOMContentLoaded", loadEmployees);

// --- FONKSİYONLAR ---

// A. Çalışanları Listele
async function loadEmployees() {
    const tableBody = document.getElementById("employeesTableBody");
    try {
        const response = await fetch(`${API_BASE_URL}/Employees`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const employees = await response.json();

        tableBody.innerHTML = "";
        employees.forEach(emp => {
            const row = `
                <tr>
                    <td>${emp.firstName} ${emp.lastName}</td>
                    <td>${emp.email}</td>
                    <td><span class="badge ${emp.role === 'Admin' ? 'bg-danger' : 'bg-secondary'}">${emp.role}</span></td>
                    <td>${emp.annualLeaveAllowance} Gün</td>
                    <td class="text-end pe-3">
                        <button class="btn btn-sm btn-info text-white me-1" onclick="viewHistory(${emp.id})" title="İzin Geçmişi"><i class="bi bi-calendar-check"></i></button>
                        <button class="btn btn-sm btn-warning text-white me-1" onclick="editEmployee(${emp.id})" title="Düzenle"><i class="bi bi-pencil"></i></button>
                        <button class="btn btn-sm btn-danger" onclick="deleteEmployee(${emp.id})" title="Sil"><i class="bi bi-trash"></i></button>
                    </td>
                </tr>
            `;
            tableBody.innerHTML += row;
        });
    } catch (error) {
        console.error(error);
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Veriler yüklenemedi.</td></tr>';
    }
}

// B. Modal Aç (Yeni Ekleme)
window.openEmployeeModal = () => {
    document.getElementById("employeeForm").reset();
    document.getElementById("empId").value = ""; // ID boşsa "Yeni Ekle" modudur
    document.getElementById("employeeModalTitle").textContent = "Yeni Çalışan Ekle";
    document.getElementById("passwordDiv").style.display = "block"; // Şifre alanı açık
    
    new bootstrap.Modal(document.getElementById('employeeModal')).show();
};

// C. Modal Aç (Düzenleme)
window.editEmployee = async (id) => {
    try {
        const response = await fetch(`${API_BASE_URL}/Employees/${id}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const emp = await response.json();

        document.getElementById("empId").value = emp.id;
        document.getElementById("firstName").value = emp.firstName;
        document.getElementById("lastName").value = emp.lastName;
        document.getElementById("email").value = emp.email;
        document.getElementById("allowance").value = emp.annualLeaveAllowance;
        document.getElementById("role").value = emp.role;
        
        // Düzenlerken şifre değiştirmeyi kapatalım (Karmaşıklık olmasın)
        document.getElementById("passwordDiv").style.display = "none";
        document.getElementById("employeeModalTitle").textContent = "Çalışan Düzenle";

        new bootstrap.Modal(document.getElementById('employeeModal')).show();
    } catch (error) {
        Swal.fire('Hata', 'Bilgiler çekilemedi', 'error');
    }
};

// D. Form Gönder (Ekle veya Güncelle)
document.getElementById("employeeForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    
    const id = document.getElementById("empId").value;
    const isEdit = id !== ""; // ID varsa Düzenleme modudur

    const payload = {
        firstName: document.getElementById("firstName").value,
        lastName: document.getElementById("lastName").value,
        email: document.getElementById("email").value,
        role: document.getElementById("role").value,
        annualLeaveAllowance: parseInt(document.getElementById("allowance").value)
    };

    if (!isEdit) {
        // Yeni ekliyorsak şifre zorunlu
        payload.password = document.getElementById("password").value;
    } else {
        payload.id = parseInt(id);
    }

    const url = isEdit ? `${API_BASE_URL}/Employees/${id}` : `${API_BASE_URL}/Employees`;
    const method = isEdit ? "PUT" : "POST";

    try {
        const response = await fetch(url, {
            method: method,
            headers: { 
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            Swal.fire('Başarılı', `Çalışan ${isEdit ? 'güncellendi' : 'eklendi'}.`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('employeeModal')).hide();
            loadEmployees(); // Listeyi yenile
        } else {
            Swal.fire('Hata', 'İşlem başarısız.', 'error');
        }
    } catch (error) {
        Swal.fire('Hata', 'Sunucu hatası.', 'error');
    }
});

// E. Silme İşlemi
window.deleteEmployee = async (id) => {
    const result = await Swal.fire({
        title: 'Emin misiniz?',
        text: "Bu çalışan ve tüm izinleri silinecek!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal'
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(`${API_BASE_URL}/Employees/${id}`, {
                method: "DELETE",
                headers: { "Authorization": `Bearer ${token}` }
            });

            if (response.ok) {
                Swal.fire('Silindi', 'Kullanıcı silindi.', 'success');
                loadEmployees();
            } else {
                Swal.fire('Hata', 'Silinemedi.', 'error');
            }
        } catch (error) {
            Swal.fire('Hata', 'Sunucu hatası.', 'error');
        }
    }
};

// F. İzin Geçmişi Görüntüleme
window.viewHistory = async (id) => {
    const tableBody = document.getElementById("historyTableBody");
    tableBody.innerHTML = '<tr><td colspan="4">Yükleniyor...</td></tr>';
    new bootstrap.Modal(document.getElementById('historyModal')).show();

    try {
        const response = await fetch(`${API_BASE_URL}/LeaveRequests/Employee/${id}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const leaves = await response.json();

        tableBody.innerHTML = "";
        if(leaves.length === 0) {
            tableBody.innerHTML = '<tr><td colspan="4" class="text-center">Kayıt yok.</td></tr>';
            return;
        }

        leaves.forEach(req => {
            let badge = req.status === 1 ? 'bg-warning' : (req.status === 2 ? 'bg-success' : 'bg-danger');
            let statusText = req.status === 1 ? 'Bekliyor' : (req.status === 2 ? 'Onaylı' : 'Red');
            
            // Tarih Hesaplama
            const start = new Date(req.startDate);
            const end = new Date(req.endDate);
            const days = Math.ceil((end - start) / (1000 * 60 * 60 * 24));

            tableBody.innerHTML += `
                <tr>
                    <td>${req.leaveType}</td>
                    <td>${start.toLocaleDateString()} - ${end.toLocaleDateString()}</td>
                    <td>${days} Gün</td>
                    <td><span class="badge ${badge}">${statusText}</span></td>
                </tr>
            `;
        });

    } catch (error) {
        tableBody.innerHTML = '<tr><td colspan="4" class="text-danger">Hata oluştu.</td></tr>';
    }


};

// G. Excel İndirme Fonksiyonu
window.downloadExcel = async () => {
    try {
        Swal.fire({ 
            title: 'Excel Hazırlanıyor...', 
            text: 'Lütfen bekleyin',
            didOpen: () => { Swal.showLoading() } 
        });

        // 1. API'ye İstek At (Response'u BLOB (Dosya) olarak alacağız)
        const response = await fetch(`${API_BASE_URL}/Employees/Export/Excel`, {
            method: "GET",
            headers: { 
                "Authorization": `Bearer ${token}` 
            }
        });

        if (response.ok) {
            // 2. Gelen veriyi "Blob"a çevir
            const blob = await response.blob();
            
            // 3. Tarayıcıda sanal bir indirme linki oluştur
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Calisanlar_Listesi.xlsx`; // İsim ver
            document.body.appendChild(a);
            
            // 4. Tıkla ve İndir
            a.click();
            
            // 5. Temizlik
            a.remove();
            window.URL.revokeObjectURL(url);
            
            Swal.close(); // Yükleniyor penceresini kapat
            
            // Opsiyonel: Küçük bir toast mesajı
            const Toast = Swal.mixin({
                toast: true, position: 'top-end', showConfirmButton: false, timer: 3000
            });
            Toast.fire({ icon: 'success', title: 'Dosya indirildi' });

        } else {
            Swal.fire('Hata', 'Dosya indirilemedi.', 'error');
        }
    } catch (error) {
        console.error(error);
        Swal.fire('Hata', 'Sunucu hatası.', 'error');
    }
};

// H. Excel Yükleme (Import) Fonksiyonu
window.uploadExcel = async () => {
    const fileInput = document.getElementById("excelInput");
    const file = fileInput.files[0];

    if (!file) return;

    // Form Verisi Oluştur
    const formData = new FormData();
    formData.append("file", file);

    try {
        Swal.fire({ 
            title: 'Veriler Yükleniyor...', 
            text: 'Lütfen bekleyin',
            didOpen: () => { Swal.showLoading() } 
        });

        // API'ye Gönder
        const response = await fetch(`${API_BASE_URL}/Employees/Import/Excel`, {
            method: "POST",
            headers: { 
                "Authorization": `Bearer ${token}` 
                // DİKKAT: Content-Type header'ı ekleme! FormData otomatik ayarlar.
            },
            body: formData
        });

        if (response.ok) {
            Swal.fire('Başarılı', 'Kayıtlar başarıyla eklendi.', 'success');
            loadEmployees(); // Tabloyu yenile
        } else {
            Swal.fire('Hata', 'Yükleme sırasında hata oluştu.', 'error');
        }
    } catch (error) {
        console.error(error);
        Swal.fire('Hata', 'Sunucu hatası.', 'error');
    } finally {
        fileInput.value = ""; // Inputu temizle ki aynı dosyayı tekrar seçebilesin
    }
};

// I. Şablon İndirme Fonksiyonu
window.downloadTemplate = async () => {
    try {
        Swal.fire({ 
            title: 'Şablon İndiriliyor...', 
            didOpen: () => { Swal.showLoading() } 
        });

        // Backend'deki yeni Template metoduna git
        const response = await fetch(`${API_BASE_URL}/Employees/Export/Template`, {
            method: "GET",
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (response.ok) {
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Personel_Yukleme_Sablonu.xlsx`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            
            Swal.close();
        } else {
            Swal.fire('Hata', 'Şablon indirilemedi.', 'error');
        }
    } catch (error) {
        console.error(error);
        Swal.fire('Hata', 'Sunucu hatası.', 'error');
    }
};