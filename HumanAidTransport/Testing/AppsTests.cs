using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using HumanAidTransport.Models;

namespace HumanAidTransport.AppsTests
{
    internal static class Validators
    {
        public static bool IsValidName(string? name) =>
            Regex.IsMatch(name ?? "", @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9\s]{2,}$");

        public static bool IsValidPassword(string? password) =>
            !string.IsNullOrWhiteSpace(password) &&
            password.Length >= 8 &&
            !password.Contains(" ");

        public static bool IsValidPhone(string? phone) =>
            Regex.IsMatch(phone ?? "", @"^\+380\d{9}$");

        public static bool IsValidDimensions(string? dim) =>
            Regex.IsMatch(dim ?? "", @"^\d+x\d+x\d+$");
    }

    // ─── Carrier ─────────────────────────────────────────────────────

    public class CarrierTests
    {
        private readonly ITestOutputHelper _output;
        public CarrierTests(ITestOutputHelper output) => _output = output;

        // Перевіряємо, що getter AverageRating динамічно перераховується при додаванні, видаленні та очищенні рейтингів
        [Fact]
        public void Carrier_AverageRating_Recalculates_WhenRatingsChange()
        {
            var carrier = new Carrier();
            Assert.Equal(1, carrier.AverageRating);

            carrier.Ratings.Add(new CarrierRating { Rating = 5 });
            carrier.Ratings.Add(new CarrierRating { Rating = 3 });
            _output.WriteLine($"After adding [5,3]: {carrier.AverageRating}");
            Assert.Equal(4, carrier.AverageRating);

            carrier.Ratings.RemoveAt(0); // залишається лише 3
            _output.WriteLine($"After removing 5: {carrier.AverageRating}");
            Assert.Equal(3, carrier.AverageRating);

            carrier.Ratings.Clear();
            _output.WriteLine($"After clearing: {carrier.AverageRating}");
            Assert.Equal(1, carrier.AverageRating);
        }

        // Гранична умова: що буде, якщо в списку некоректні значення (< 0)
        // Модель не має валідації — тест документує поточну поведінку
        [Fact]
        public void Carrier_AverageRating_DoesNotCrash_WithInvalidRatings()
        {
            var carrier = new Carrier();
            carrier.Ratings.Add(new CarrierRating { Rating = -10 });
            carrier.Ratings.Add(new CarrierRating { Rating = 0 });

            _output.WriteLine($"Invalid ratings avg: {carrier.AverageRating}");
            Assert.True(carrier.AverageRating <= 1);
        }

        // Гранична умова: рейтинг з одного запису — не має ділення на нуль, коли 1 значення
        [Fact]
        public void Carrier_AverageRating_SingleRating_ReturnsThatValue()
        {
            var carrier = new Carrier();
            carrier.Ratings.Add(new CarrierRating { Rating = 4 });

            _output.WriteLine($"Single rating avg: {carrier.AverageRating}");
            Assert.Equal(4, carrier.AverageRating);
        }

        // Два різні екземпляри Carrier мають незалежні колекції рейтингів
        [Fact]
        public void TwoCarriers_ShouldNotShareSameRatingsCollection()
        {
            var carrier1 = new Carrier();
            var carrier2 = new Carrier();

            carrier1.Ratings.Add(new CarrierRating { Rating = 5 });

            _output.WriteLine($"carrier1: {carrier1.Ratings.Count}, carrier2: {carrier2.Ratings.Count}");
            Assert.Single(carrier1.Ratings);
            Assert.Empty(carrier2.Ratings);
            Assert.NotSame(carrier1.Ratings, carrier2.Ratings);
        }
    }

    // ─── HumanitarianAid ─────────────────────────────────────────────

    public class HumanitarianAidTests
    {
        private readonly ITestOutputHelper _output;
        public HumanitarianAidTests(ITestOutputHelper output) => _output = output;

        // Модель задає Status = "New" і Quantity = 1 за замовчуванням — перевіряємо обидва дефолти в одному тесті
        [Fact]
        public void HumanitarianAid_Defaults_StatusAndQuantity()
        {
            var task = new HumanitarianAid();

            _output.WriteLine($"Status: {task.Status}, Quantity: {task.Quantity}");
            Assert.Equal("New", task.Status);
            Assert.Equal(1, task.Quantity);
        }

        // Перевіряємо, що статус можна оновити — імітуємо зміну стану завдання
        [Fact]
        public void HumanitarianAid_Status_CanBeUpdated()
        {
            var task = new HumanitarianAid();

            task.Status = "В очікуванні";
            _output.WriteLine($"Status after update: {task.Status}");
            Assert.Equal("В очікуванні", task.Status);

            task.Status = "Виконано";
            Assert.Equal("Виконано", task.Status);
        }

        // Гранична умова: Payment = 0 — безкоштовне завдання є валідним
        [Fact]
        public void HumanitarianAid_Payment_ZeroIsAllowed()
        {
            var task = new HumanitarianAid { Payment = 0 };

            _output.WriteLine($"Payment: {task.Payment}");
            Assert.Equal(0, task.Payment);
        }

        // Notification змінює статус — перевіряємо простий life-cycle
        [Fact]
        public void Notification_Status_ShouldChangeCorrectly()
        {
            var notification = new Notification { Status = "Unread" };
            notification.Status = "Read";

            _output.WriteLine($"Notification status: {notification.Status}");
            Assert.Equal("Read", notification.Status);
        }
    }

    // ─── Volunteer ───────────────────────────────────────────────────

    public class VolunteerTests
    {
        private readonly ITestOutputHelper _output;
        public VolunteerTests(ITestOutputHelper output) => _output = output;

        // Додаємо кілька завдань і перевіряємо кількість
        [Fact]
        public void Volunteer_AddTask_MultipleTasks_WorkCorrectly()
        {
            var v = new Volunteer();
            v.Tasks.Add(new HumanitarianAid { Name = "Їжа" });
            v.Tasks.Add(new HumanitarianAid { Name = "Ліки" });

            _output.WriteLine($"Tasks count: {v.Tasks.Count}");
            Assert.Equal(2, v.Tasks.Count);
        }

        // Гранична умова: колекція Tasks після Clear повертається до нуля
        [Fact]
        public void Volunteer_TasksList_IsMutable()
        {
            var v = new Volunteer();
            v.Tasks.Add(new HumanitarianAid());
            v.Tasks.Clear();

            _output.WriteLine($"Tasks after clear: {v.Tasks.Count}");
            Assert.Empty(v.Tasks);
        }

        // Перевіряємо, що два волонтери мають незалежні списки завдань
        [Fact]
        public void TwoVolunteers_ShouldNotShareTasksList()
        {
            var v1 = new Volunteer();
            var v2 = new Volunteer();

            v1.Tasks.Add(new HumanitarianAid { Name = "Одяг" });

            _output.WriteLine($"v1 tasks: {v1.Tasks.Count}, v2 tasks: {v2.Tasks.Count}");
            Assert.Single(v1.Tasks);
            Assert.Empty(v2.Tasks);
        }
    }

    // ─── DeliveryRequest ─────────────────────────────────────────────

    public class DeliveryRequestTests
    {
        private readonly ITestOutputHelper _output;
        public DeliveryRequestTests(ITestOutputHelper output) => _output = output;

        // Гранична умова: новий DeliveryRequest без присвоєнь — навігаційні
        // властивості мають бути null (немає неочікуваних дефолтів)
        [Fact]
        public void DeliveryRequest_ShouldHandleNullRelations()
        {
            var request = new DeliveryRequest();

            _output.WriteLine("Checking null safety on Carrier and HumanitarianAid");
            Assert.Null(request.Carrier);
            Assert.Null(request.HumanitarianAid);
        }

        // Перевіряємо коректне зв'язування перевізника та завдання
        [Fact]
        public void DeliveryRequest_ShouldLinkEntitiesCorrectly()
        {
            var carrier = new Carrier { Id = 10 };
            var task = new HumanitarianAid { HumanAidId = 20 };

            var request = new DeliveryRequest
            {
                Carrier = carrier,
                HumanitarianAid = task
            };

            _output.WriteLine($"CarrierId: {request.Carrier?.Id}, AidId: {request.HumanitarianAid?.HumanAidId}");
            Assert.Equal(10, request.Carrier?.Id);
            Assert.Equal(20, request.HumanitarianAid?.HumanAidId);
        }

        // Гранична умова: CarrierRating nullable — до підтвердження оцінки немає
        [Fact]
        public void DeliveryRequest_CarrierRating_IsNullByDefault()
        {
            var request = new DeliveryRequest();

            _output.WriteLine($"CarrierRating: {request.CarrierRating?.ToString() ?? "null"}");
            Assert.Null(request.CarrierRating);
        }
    }

    // ─── TransportOrder ──────────────────────────────────────────────

    public class TransportOrderTests
    {
        private readonly ITestOutputHelper _output;
        public TransportOrderTests(ITestOutputHelper output) => _output = output;

        // Перевіряємо зміну статусу через весь life-cycle замовлення
        [Fact]
        public void TransportOrder_Status_ShouldChangeCorrectly()
        {
            var order = new TransportOrder();

            order.Status = "В процесі";
            order.Status = "Виконано";

            _output.WriteLine($"Final status: {order.Status}");
            Assert.Equal("Виконано", order.Status);
        }

        // Гранична умова: Payment nullable — замовлення без оплати є допустимим
        [Fact]
        public void TransportOrder_Payment_NullIsAllowed()
        {
            var order = new TransportOrder { Payment = null };

            _output.WriteLine($"Payment: {order.Payment?.ToString() ?? "null"}");
            Assert.Null(order.Payment);
        }
    }

    // ─── Validators ──────────────────────────────────────────────────

    public class ValidationTests
    {
        private readonly ITestOutputHelper _output;
        public ValidationTests(ITestOutputHelper output) => _output = output;

        // Граничні умови для імені: мінімум 2 символи, без спецсимволів
        [Theory]
        [InlineData("Іван", true)]
        [InlineData("Ivan123", true)]
        [InlineData("А", false)] // 1 символ — менше мінімуму
        [InlineData("", false)]
        [InlineData("!@#", false)] // лише спецсимволи
        public void Name_BoundaryAndInvalidCases(string name, bool expected)
        {
            var result = Validators.IsValidName(name);
            _output.WriteLine($"Name: '{name}' → {result}");
            Assert.Equal(expected, result);
        }

        // Граничні умови для пароля: рівно 8 символів — мінімально допустимий
        [Theory]
        [InlineData("pass1234", true)]  // рівно 8 — граничний мінімум
        [InlineData("pass123", false)] // 7 символів — на 1 менше мінімуму
        [InlineData("pass 1234", false)] // пробіл — заборонено
        [InlineData("", false)]
        public void Password_BoundaryAndInvalidCases(string password, bool expected)
        {
            var result = Validators.IsValidPassword(password);
            _output.WriteLine($"Password: '{password}' → {result}");
            Assert.Equal(expected, result);
        }

        // Граничні умови для телефону: формат +380XXXXXXXXX (13 символів)
        [Theory]
        [InlineData("+380991234567", true)]
        [InlineData("0991234567", false)] // без +380
        [InlineData("+38099123456", false)] // 8 цифр замість 9
        [InlineData("+380991234567x", false)] // зайвий символ
        [InlineData("", false)]
        public void Phone_BoundaryAndInvalidCases(string phone, bool expected)
        {
            var result = Validators.IsValidPhone(phone);
            _output.WriteLine($"Phone: '{phone}' → {result}");
            Assert.Equal(expected, result);
        }

        // Граничні умови для розмірів: формат NxNxN — тільки цифри та 'x'
        [Theory]
        [InlineData("10x3x3", true)]
        [InlineData("10-10-10", false)] // дефіси замість 'x'
        [InlineData("10x10", false)] // лише два виміри
        [InlineData("axbxc", false)] // літери замість цифр
        [InlineData("", false)]
        public void Dimensions_BoundaryAndInvalidCases(string dim, bool expected)
        {
            var result = Validators.IsValidDimensions(dim);
            _output.WriteLine($"Dimensions: '{dim}' → {result}");
            Assert.Equal(expected, result);
        }
    }
}

// ─── Додаткові тести ─────────────────────────────────────────────

public class CarrierRatingTests
{
    private readonly ITestOutputHelper _output;
    public CarrierRatingTests(ITestOutputHelper output) => _output = output;

    // Перевіряємо, що CarrierRating зберігає поля коректно
    [Fact]
    public void CarrierRating_Fields_AreStoredCorrectly()
    {
        var rating = new CarrierRating { Id = 1, CarrierId = 42, Rating = 5 };

        _output.WriteLine($"CarrierId: {rating.CarrierId}, Rating: {rating.Rating}");
        Assert.Equal(42, rating.CarrierId);
        Assert.Equal(5, rating.Rating);
    }

    // Гранична умова: всі оцінки максимальні — середнє має бути 5
    [Fact]
    public void Carrier_AverageRating_AllFives_ReturnsMax()
    {
        var carrier = new Carrier();
        carrier.Ratings.Add(new CarrierRating { Rating = 5 });
        carrier.Ratings.Add(new CarrierRating { Rating = 5 });
        carrier.Ratings.Add(new CarrierRating { Rating = 5 });

        _output.WriteLine($"All 5s average: {carrier.AverageRating}");
        Assert.Equal(5, carrier.AverageRating);
    }
}

public class HumanitarianAidExtraTests
{
    private readonly ITestOutputHelper _output;
    public HumanitarianAidExtraTests(ITestOutputHelper output) => _output = output;

    // Перевіряємо, що enum AidType присвоюється і зчитується коректно
    [Theory]
    [InlineData(AidType.Food)]
    [InlineData(AidType.Medicine)]
    [InlineData(AidType.Military)]
    public void HumanitarianAid_AidType_IsSetCorrectly(AidType type)
    {
        var task = new HumanitarianAid { Type = type };

        _output.WriteLine($"AidType: {task.Type}");
        Assert.Equal(type, task.Type);
    }

    // Перевіряємо, що enum Priority присвоюється коректно для всіх рівнів
    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    public void HumanitarianAid_PriorityLevel_IsSetCorrectly(Priority priority)
    {
        var task = new HumanitarianAid { PriorityLevel = priority };

        _output.WriteLine($"Priority: {task.PriorityLevel}");
        Assert.Equal(priority, task.PriorityLevel);
    }

    // Гранична умова: велике значення Quantity — модель не обмежує зверху
    [Fact]
    public void HumanitarianAid_LargeQuantity_IsAllowed()
    {
        var task = new HumanitarianAid { Quantity = 999999 };

        _output.WriteLine($"Large quantity: {task.Quantity}");
        Assert.Equal(999999, task.Quantity);
    }
}

public class NotificationExtraTests
{
    private readonly ITestOutputHelper _output;
    public NotificationExtraTests(ITestOutputHelper output) => _output = output;

    // Перевіряємо, що CreatedAt заповнюється автоматично при створенні
    // і відповідає поточному часу UTC
    [Fact]
    public void Notification_CreatedAt_IsSetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var notification = new Notification();
        var after = DateTime.UtcNow;

        _output.WriteLine($"CreatedAt: {notification.CreatedAt}");
        Assert.InRange(notification.CreatedAt, before, after);
    }
}   