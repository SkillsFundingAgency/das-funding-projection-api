CREATE TABLE dbo.[EmployerFundingProjection]
(
    [EmployerAccountId]             BIGINT          NOT NULL,
    [CommittedLearnerCostTotal]     DECIMAL(18, 2)  NOT NULL    DEFAULT 0,
    [CommittedTransferOutTotal]     DECIMAL(18, 2)  NOT NULL    DEFAULT 0,
    [LastRecalculatedDate]          DATETIME2       NOT NULL,
    [CreatedDate]                   DATETIME2       NOT NULL    DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_EmployerFundingProjection]
    PRIMARY KEY CLUSTERED ([EmployerAccountId])
    )