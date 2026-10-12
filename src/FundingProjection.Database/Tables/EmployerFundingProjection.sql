CREATE TABLE dbo.[EmployerFundingProjection]
(
    [Id]                            UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [EmployerAccountId]             BIGINT              NOT NULL,
    [CommittedLearnerCostTotal]     DECIMAL(18, 2)      NOT NULL    DEFAULT 0,
    [CommittedTransferOutTotal]     DECIMAL(18, 2)      NOT NULL    DEFAULT 0,
    [CalendarPeriodMonth]           INT                 NOT NULL,
    [CalendarPeriodYear]            INT                 NOT NULL,
    [LastRecalculatedDate]          DATETIME2           NOT NULL,
    [CreatedDate]                   DATETIME2           NOT NULL    DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_EmployerFundingProjection]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [UQ_EmployerFundingProjection_EmployerMonth]
        UNIQUE ([EmployerAccountId], [CalendarPeriodYear], [CalendarPeriodMonth]),

    CONSTRAINT [CK_Month_Range]
        CHECK ([CalendarPeriodMonth] >= 1 AND [CalendarPeriodMonth] <= 12),

    CONSTRAINT [CK_Year_Valid]
        CHECK ([CalendarPeriodYear] >= 2000)
)
GO

CREATE NONCLUSTERED INDEX [IX_EmployerFundingProjection_EmployerAccount]
    ON [dbo].[EmployerFundingProjection] ([EmployerAccountId])
    INCLUDE ([CommittedLearnerCostTotal], [CommittedTransferOutTotal])
GO

CREATE NONCLUSTERED INDEX [IX_EmployerFundingProjection_Period]
    ON [dbo].[EmployerFundingProjection] ([CalendarPeriodYear], [CalendarPeriodMonth])
    INCLUDE ([CommittedLearnerCostTotal])
GO