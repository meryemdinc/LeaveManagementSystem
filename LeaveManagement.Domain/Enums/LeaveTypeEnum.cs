

namespace LeaveManagement.Domain.Enums
{
    public enum LeaveTypeEnum
    {
        Annual = 1,    // Yıllık İzin (Bakiyeden düşer)
        Sick = 2,      // Hastalık İzni
        Unpaid = 3     // Ücretsiz İzin
        // Diğerleri veritabanına eklenebilir ama kod tarafında özel logic gerektirenler bunlar.
    }
}
