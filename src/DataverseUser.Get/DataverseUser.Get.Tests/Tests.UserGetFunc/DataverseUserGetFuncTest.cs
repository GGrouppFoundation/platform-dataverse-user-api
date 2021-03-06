using System;
using System.Threading;
using System.Threading.Tasks;
using GGroupp.Infra;
using Moq;
using PrimeFuncPack;

namespace GGroupp.Platform.Dataverse.DataverseUser.Get.Tests;

using IDataverseUserGetFunc = IAsyncValueFunc<DataverseUserGetIn, Result<DataverseUserGetOut, Failure<DataverseUserGetFailureCode>>>;

public sealed partial class DataverseUserGetFuncTest
{
    static DataverseUserGetFuncTest()
        =>
        SomeActiveDirectoryGuid = Guid.Parse("1203c0e2-3648-4596-80dd-127fdd2610b7");
    
    private static readonly Guid SomeActiveDirectoryGuid;

    private static IDataverseUserGetFunc CreateFunc(IDataverseEntityGetSupplier dataverseEntityGetSupplier)
        =>
        Dependency.Of(dataverseEntityGetSupplier)
        .UseUserGetApi()
        .Resolve(Mock.Of<IServiceProvider>());

    private static Mock<IDataverseEntityGetSupplier> CreateMockDataverseApiClient(
        Result<DataverseEntityGetOut<UserJsonGetOut>, Failure<DataverseFailureCode>> result,
        Action<DataverseEntityGetIn>? callback = default)
    {
        var mock = new Mock<IDataverseEntityGetSupplier>();

        var m = mock.Setup(
            s => s.GetEntityAsync<UserJsonGetOut>(It.IsAny<DataverseEntityGetIn>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<DataverseEntityGetOut<UserJsonGetOut>, Failure<DataverseFailureCode>>>(result));

        if (callback is not null)
        {
            m.Callback<DataverseEntityGetIn, CancellationToken>(
                (@in, _) => callback.Invoke(@in));
        }

        return mock;
    }
}