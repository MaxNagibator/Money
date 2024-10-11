using Microsoft.EntityFrameworkCore;
using Money.Api.Tests.TestTools;
using Money.Api.Tests.TestTools.Entities;
using Money.ApiClient;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.Payments;

public class PaymentTests
{
    private DatabaseClient _dbClient;
    private TestUser _user;
    private MoneyClient _apiClient;

    [SetUp]
    public void Setup()
    {
        _dbClient = Integration.GetDatabaseClient();
        _user = _dbClient.WithUser();
        _apiClient = new MoneyClient(Integration.GetHttpClient(), Console.WriteLine);
        _apiClient.SetUser(_user);
    }

    [Test]
    public async Task GetTest()
    {
        // todo айди платежа начинается не с единицы
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment(),
            category.WithPayment(),
            category.WithPayment(),
        ];

        _dbClient.Save();

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get().IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.GreaterThanOrEqualTo(payments.Length));

        TestPayment[] testCategories = payments.ExceptBy(apiPayments.Select(x => x.Id), payment => payment.Id).ToArray();
        Assert.That(testCategories, Is.Not.Null);
        Assert.That(testCategories, Is.Empty);
    }

    [Test]
    public async Task GetByDateFromTest()
    {
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment().SetDate(DateTime.Now.Date.AddMonths(1)),
            category.WithPayment(),
        ];

        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateFrom = DateTime.Now.Date.AddMonths(1),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Date, Is.GreaterThanOrEqualTo(DateTime.Now.Date));
    }

    [Test]
    public async Task GetByDateToTest()
    {
        TestCategory category = _user.WithCategory();

        TestPayment[] payments =
        [
            category.WithPayment().SetDate(DateTime.Now.Date.AddMonths(1)),
            category.WithPayment().SetDate(DateTime.Now.Date),
        ];

        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateTo = DateTime.Now.Date.AddDays(1),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Date, Is.GreaterThanOrEqualTo(DateTime.Now.Date));
        Assert.That(apiPayments[0].Date, Is.LessThan(DateTime.Now.Date.AddDays(1)));
    }

    [Test]
    public async Task GetByDateRangeTest()
    {
        TestCategory category = _user.WithCategory();
        category.WithPayment().SetDate(DateTime.Now.Date.AddDays(-1));
        category.WithPayment().SetDate(DateTime.Now.Date);
        category.WithPayment().SetDate(DateTime.Now.Date.AddDays(1));
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateFrom = DateTime.Now.Date.AddDays(-1),
            DateTo = DateTime.Now.Date.AddDays(2),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task GetByCategoryIdsTest()
    {
        _user.WithCategory().WithPayment();
        TestCategory category = _user.WithCategory().WithPayment().Category;
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            CategoryIds = [category.Id],
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task GetByCommentTest()
    {
        TestCategory category = _user.WithCategory();
        TestPayment payment = category.WithPayment().SetComment("ochen vazhniy platezh");
        category.WithPayment();
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            Comment = payment.Comment[..10],
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Comment, Is.EqualTo(payment.Comment));
    }

    [Test]
    public async Task GetByPlaceTest()
    {
        TestPlace place = _user.WithPlace();
        TestCategory category = _user.WithCategory();
        TestPayment payment = category.WithPayment();
        payment.SetPlace(place);
        category.WithPayment();
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            Place = place.Name,
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(1));
        Assert.That(apiPayments[0].Place, Is.EqualTo(place.Name));
    }

    [Test]
    public async Task GetByMultipleCategoryIdsTest()
    {
        TestCategory category1 = _user.WithCategory();
        TestCategory category2 = _user.WithCategory();
        category1.WithPayment();
        category2.WithPayment();
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            CategoryIds = [category1.Id, category2.Id],
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetNoPaymentsTest()
    {
        _dbClient.Save();

        PaymentClient.PaymentFilterDto filter = new()
        {
            DateFrom = DateTime.Now.AddDays(1),
        };

        PaymentClient.Payment[]? apiPayments = await _apiClient.Payment.Get(filter).IsSuccessWithContent();
        Assert.That(apiPayments, Is.Not.Null);
        Assert.That(apiPayments, Is.Empty);
    }

    [Test]
    public async Task GetByIdTest()
    {
        TestPlace place = _user.WithPlace();
        TestPayment payment = _user.WithPayment().SetPlace(place);
        _dbClient.Save();

        PaymentClient.Payment? apiPayemnt = await _apiClient.Payment.GetById(payment.Id).IsSuccessWithContent();

        Assert.That(apiPayemnt, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(apiPayemnt.Id, Is.EqualTo(payment.Id));
            Assert.That(apiPayemnt.Comment, Is.EqualTo(payment.Comment));
            Assert.That(apiPayemnt.CategoryId, Is.EqualTo(payment.Category.Id));
            Assert.That(apiPayemnt.Date, Is.EqualTo(payment.Date));
            Assert.That(apiPayemnt.Place, Is.EqualTo(payment.Place.Name));
        });
    }

    [Test]
    public async Task CreateTest()
    {
        var category = _user.WithCategory();
        _dbClient.Save();

        TestPayment payment = _user.WithPayment();
        TestPlace place = _user.WithPlace();

        PaymentClient.SaveRequest request = new()
        {
            CategoryId = category.Id,
            Sum = payment.Sum,
            Date = payment.Date,
            Comment = payment.Comment,
            Place = place.Name,
        };

        int createdId = await _apiClient.Payment.Create(request).IsSuccessWithContent();
        Payment? dbPayment = _dbClient.CreateApplicationDbContext().Payments.SingleOrDefault(_user.Id, createdId);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.That(dbPayment, Is.Not.Null);
        Assert.That(dbPlace, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbPayment.Date, Is.EqualTo(request.Date));
            Assert.That(dbPayment.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbPayment.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbPayment.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
        });
    }

    [Test]
    public async Task UpdateTest()
    {
        TestPayment payment = _user.WithPayment();
        TestCategory updatedCategory = _user.WithCategory();
        _dbClient.Save();

        TestPlace place = _user.WithPlace();
        TestPayment updatedPayment = _user.WithPayment();

        PaymentClient.SaveRequest request = new()
        {
            Comment = updatedPayment.Comment,
            CategoryId = updatedCategory.Id,
            Date = updatedPayment.Date,
            Place = place.Name,
            Sum = updatedPayment.Sum,
        };

        await _apiClient.Payment.Update(payment.Id, request).IsSuccess();
        Payment? dbPayment = _dbClient.CreateApplicationDbContext().Payments.SingleOrDefault(_user.Id, payment.Id);
        Place? dbPlace = _dbClient.CreateApplicationDbContext().Places.FirstOrDefault(x => x.UserId == _user.Id && x.Name == place.Name);

        Assert.That(dbPayment, Is.Not.Null);
        Assert.That(dbPlace, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(dbPayment.Date, Is.EqualTo(request.Date));
            Assert.That(dbPayment.Sum, Is.EqualTo(request.Sum));
            Assert.That(dbPayment.Comment, Is.EqualTo(request.Comment));
            Assert.That(dbPayment.CategoryId, Is.EqualTo(request.CategoryId));
            Assert.That(dbPlace.Name, Is.EqualTo(request.Place));
        });
    }

    [Test]
    public async Task DeleteTest()
    {
        TestPayment Payment = _user.WithPayment();
        _dbClient.Save();

        await _apiClient.Payment.Delete(Payment.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        Payment? dbPayment = context.Payments.SingleOrDefault(_user.Id, Payment.Id);

        Assert.That(dbPayment, Is.Null);

        dbPayment = context.Payments
            .IgnoreQueryFilters()
            .SingleOrDefault(_user.Id, Payment.Id);

        Assert.That(dbPayment, Is.Not.Null);
        Assert.That(dbPayment.IsDeleted, Is.EqualTo(true));
    }

    [Test]
    public async Task RestoreTest()
    {
        TestPayment Payment = _user.WithPayment().SetIsDeleted();
        _dbClient.Save();

        await _apiClient.Payment.Restore(Payment.Id).IsSuccess();

        await using ApplicationDbContext context = _dbClient.CreateApplicationDbContext();

        Payment? dbPayment = context.Payments.SingleOrDefault(_user.Id, Payment.Id);
        Assert.That(dbPayment, Is.Not.Null);
        Assert.That(dbPayment.IsDeleted, Is.EqualTo(false));
    }
}
