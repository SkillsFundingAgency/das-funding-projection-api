CREATE TABLE dbo.[CommittedLearners]
(
    [Id]                    UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [EmployerAccountId]     BIGINT              NOT NULL,
    [ApprenticeshipId]      BIGINT              NOT NULL,
    [CommitmentId]          BIGINT              NULL,
    [Cost]                  DECIMAL(18, 2)      NOT NULL,
    [StartDate]             DATE                NOT NULL,
    [EndDate]               DATE                NOT NULL,
    [PaymentStatus]         NVARCHAR(50)        NOT NULL,    -- 'Active', 'Completed', 'Withdrawn', 'Paused'
    [CreatedDate]           DATETIME2           NOT NULL,
    [LastUpdatedDate]       DATETIME2           NOT NULL,
    [ImportedDate]          DATETIME2           NOT NULL,
    [ImportStatus] 	        NVARCHAR(50)        NOT NULL,    --  'Pending', 'Imported', 'Failed'

    CONSTRAINT [PK_CommittedLearners]
    PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [UQ_CommittedLearners_Account_Apprenticeship]
    UNIQUE ([EmployerAccountId], [ApprenticeshipId])
    )

    GO

CREATE NONCLUSTERED INDEX [IX_CommittedLearners_EmployerAccountId]
    ON [dbo].[CommittedLearners] ([EmployerAccountId])
    INCLUDE ([Cost], [PaymentStatus])
GO