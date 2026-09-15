CREATE TABLE dbo.[CommittedLearnerCost]
(
    [Id]                    UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [EmployerAccountId]     BIGINT              NOT NULL,
    [ApprenticeshipId]      BIGINT              NOT NULL,
    [TransferSenderId]      BIGINT              NULL,        -- NULL = levy-funded by own account; NOT NULL = funded via transfer in (excluded from total)
    [RemainingCost]         DECIMAL(18, 2)      NOT NULL,
    [PlannedEndDate]        DATE                NOT NULL,
    [Status]                NVARCHAR(50)        NOT NULL,    -- 'Active', 'Completed', 'Withdrawn', 'Paused'
    [LastUpdatedDate]       DATETIME2           NOT NULL,

    CONSTRAINT [PK_CommittedLearnerCost]
    PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [UQ_CommittedLearnerCost_Account_Apprenticeship]
    UNIQUE ([EmployerAccountId], [ApprenticeshipId])
    )

    GO

CREATE NONCLUSTERED INDEX [IX_CommittedLearnerCost_EmployerAccountId]
    ON [dbo].[CommittedLearnerCost] ([EmployerAccountId])
    INCLUDE ([RemainingCost], [TransferSenderId], [Status])