using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using GGroupp.Infra;

namespace GGroupp.Platform;

partial class DataverseUserGetFunc
{
    public partial ValueTask<Result<DataverseUserGetOut, Failure<DataverseUserGetFailureCode>>> InvokeAsync(
        DataverseUserGetIn input, CancellationToken cancellationToken)
        =>
        AsyncPipeline.Pipe(
            input, cancellationToken)
        .HandleCancellation()
        .Pipe(
            @in => new DataverseEntityGetIn(
                entityPluralName: ApiNames.SystemUserEntityName,
                entityKey: BuildAlternateKey(input.ActiveDirectoryUserId),
                selectFields: selectedFields))
        .PipeValue(
            entityGetSupplier.GetEntityAsync<UserJsonGetOut>)
        .MapFailure(
            failure => failure.MapFailureCode(MapDataverseFailureCode))
        .MapSuccess(
            entityGetOut => new DataverseUserGetOut(
                systemUserId: entityGetOut.Value.SystemUserId,
                firstName: entityGetOut.Value.FirstName,
                lastName: entityGetOut.Value.LastName,
                fullName: entityGetOut.Value.FullName));

    private static DataverseAlternateKey BuildAlternateKey(Guid activeDirectoryId)
        => 
        new(
            new KeyValuePair<string, string>[]
            {
                new(ApiNames.ActiveDirectoryObjectIdFieldName, activeDirectoryId.ToString("D", CultureInfo.InvariantCulture))
            });

    private static DataverseUserGetFailureCode MapDataverseFailureCode(DataverseFailureCode dataverseFailureCode)
        =>
        dataverseFailureCode switch
        {
            DataverseFailureCode.RecordNotFound => DataverseUserGetFailureCode.NotFound,
            _ => DataverseUserGetFailureCode.Unknown
        };
}