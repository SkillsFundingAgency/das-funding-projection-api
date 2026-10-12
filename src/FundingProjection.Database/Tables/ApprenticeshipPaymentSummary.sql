CREATE TABLE dbo.[ApprenticeshipPaymentSummaries]
(
    [ApprenticeshipId]   BIGINT         NOT NULL,
    [AccountId]          BIGINT         NOT NULL,
    [Uln]                BIGINT         NOT NULL,
    [Dob]                DATE           NULL,
    [Status]             NVARCHAR(30)   NOT NULL,
    [TotalCost]          DECIMAL(18,2)  NOT NULL,
    [TotalPaid]          DECIMAL(18,2)  NOT NULL DEFAULT 0,
    [StartDate]          DATE           NOT NULL,
    [EndDate]            DATE           NOT NULL,
    [LastPaymentDate]    DATE           NULL,
    [LastPaymentAmount]  DECIMAL(18,2)  NULL,
    [LastUpdatedDate]    DATETIME2      NOT NULL DEFAULT GETDATE(),

    CONSTRAINT [PK_ApprenticeshipPaymentSummaries] PRIMARY KEY CLUSTERED ([ApprenticeshipId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_ApprenticeshipPaymentSummaries_AccountId] ON [dbo].[ApprenticeshipPaymentSummaries] ([AccountId])
GO