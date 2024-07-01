using JumpStartCS.Orleans.Client.Contracts;
using JumpStartCS.Orleans.Grains.Abstractions;
using JumpStartCS.Orleans.Grains.Filters;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient((context, client) =>
{
    client.UseAzureStorageClustering(configureOptions: options =>
    {
        options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
    });

    client.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "JumpstartCSCluster";
        options.ServiceId = "JumpstartCSService";
    });

    client.AddOutgoingGrainCallFilter<LoggingOutgoingGrainCallFilter>();

    client.UseTransactions();
});

var app = builder.Build();

app.MapGet("checkingaccount/{checkingAccountId}/balance", 
    async (
        Guid checkingAccountId,
        ITransactionClient transactionClient,
        IClusterClient clusterClient) => 
    {
        decimal balance = 0;

        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

            balance = await checkingAccountGrain.GetBalance();
        });

        return TypedResults.Ok(balance);
    });

app.MapPost("checkingaccount", async (
    CreateAccount createAccount,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    var checkingAccountId = Guid.NewGuid();

    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

        await checkingAccountGrain.Initialise(createAccount.OpeningBalance);
    });

    return TypedResults.Created($"checkingaccounnt/{checkingAccountId}");
});

app.MapPost("checkingaccount/{checkingAccountId}/debit", async (
    Guid checkingAccountId,
    Debit debit,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

        await checkingAccountGrain.Debit(debit.Amount);
    });

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/credit", async (
    Guid checkingAccountId,
    Credit credit,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

        await checkingAccountGrain.Credit(credit.Amount);
    });

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/recurringPayment", async (
    Guid checkingAccountId,
    CreateRecurringPayment createRecurringPayment,
    IClusterClient clusterClient) =>
{
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

    await checkingAccountGrain.AddReccuringPayment(createRecurringPayment.PaymentId,
        createRecurringPayment.PaymentAmount,
        createRecurringPayment.PaymentRecurrsEveryMinutes);

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/fireandforgetwork", async (
    Guid checkingAccountId,
    IClusterClient clusterClient) =>
{
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

    await checkingAccountGrain.FireAndForgetWork();

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountId}/cancellablework", async (
    Guid checkingAccountId,
    IClusterClient clusterClient) =>
{
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountId);

    var grainCancellationTokenSource = new GrainCancellationTokenSource();

    var grainCallTask = checkingAccountGrain.CancelableWork(grainCancellationTokenSource.Token, 15);

    //var cancelWorkTask = async () =>
    //{
    //    await Task.Delay(TimeSpan.FromSeconds(5));

    //    await grainCancellationTokenSource.Cancel();
    //};

    await Task.WhenAll(grainCallTask);

    return TypedResults.NoContent();
});


app.MapPost("atm", async (
    CreateAtm createAtm,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    var atmId = Guid.NewGuid();

    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);

        await atmGrain.Initialise(createAtm.InitialAtmCashBalance);
    });

    return TypedResults.Created($"atm/{atmId}");
});

app.MapGet("atm/{atmId}/balance", async (
    Guid atmId,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    decimal balance = 0;

    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);

        balance = await atmGrain.GetBalance();
    });

    return TypedResults.Ok(balance);
});

app.MapPost("atm/{atmId}/withdrawl", async (
    Guid atmId,
    AtmWithdrawl atmWithdrawl,
    ITransactionClient transactionClient,
    IClusterClient clusterClient) =>
{
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmId);

        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(atmWithdrawl.CheckingAccountId);

        await atmGrain.Withdraw(atmWithdrawl.CheckingAccountId, atmWithdrawl.Amount);

        await checkingAccountGrain.Debit(atmWithdrawl.Amount);
    });
});

app.MapGet("customer/{customerId}/networth", async (
    Guid customerId,
    IClusterClient clusterClient) =>
{
    var customerGrain = clusterClient.GetGrain<ICustomerGrain>(customerId);

    var netWorth = await customerGrain.GetNetWorth();

    return TypedResults.Ok(netWorth);
});

app.MapPost("customer/{customerId}/addcheckingaccount", async (
    Guid customerId,
    CustomerCheckingAccount customerCheckingAccount,
    IClusterClient clusterClient) =>
{
    var customerGrain = clusterClient.GetGrain<ICustomerGrain>(customerId);

    await customerGrain.AddCheckingAccount(customerCheckingAccount.AccountId);

    return TypedResults.NoContent();
});

app.MapPost("transfer", async (
    Transfer transfer,
    IClusterClient clusterClient) =>
{
    var statlessTransferProcessingGrain = clusterClient.GetGrain<IStatlessTransferProcessingGrain>(0);

    await statlessTransferProcessingGrain.ProcessTransfer(transfer.FromAccountId, transfer.ToAccountId, transfer.Amount);

    return TypedResults.NoContent();
});

app.Run();
