CREATE TABLE dbo.[ApprenticeshipPaymentSummaries]
(
    [ApprenticeshipId]   BIGINT         NOT NULL,
    [AccountId]          BIGINT         NOT NULL,   -- the account whose levy funds the apprenticeship
    [IsTransfer]         BIT            NOT NULL DEFAULT 0,   -- 1 = AccountId funds another employer's apprentice via a levy transfer
    [Uln]                BIGINT         NOT NULL,
    [AgeAtStart]         TINYINT        NOT NULL,
    [Status]             NVARCHAR(30)   NOT NULL,
    [TotalCost]          DECIMAL(18,2)  NOT NULL,
    [TotalPaid]          DECIMAL(18,2)  NOT NULL DEFAULT 0,
    [StartDate]          DATE           NOT NULL,
    [EndDate]            DATE           NOT NULL,
    [ProviderOrder]      SMALLINT       NULL,
    [LastPaymentDate]    DATE           NULL,
    [LastPaymentAmount]  DECIMAL(18,2)  NULL,
    [LastUpdatedDate]    DATETIME2      NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_ApprenticeshipPaymentSummaries] PRIMARY KEY CLUSTERED ([ApprenticeshipId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_ApprenticeshipPaymentSummaries_AccountId] ON [dbo].[ApprenticeshipPaymentSummaries] ([AccountId])
GO