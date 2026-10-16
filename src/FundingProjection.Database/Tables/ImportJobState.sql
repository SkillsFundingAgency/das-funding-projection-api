CREATE TABLE dbo.[ImportJobState](
    [Id]                           UniqueIdentifier NOT NULL DEFAULT NEWSEQUENTIALID(),
    [AccountId]                    BIGINT           NOT NULL,
    [JobName]                      NVARCHAR(100)    NOT NULL,
    [JobState]                     NVARCHAR(30)     NOT NULL,
    [LastSuccessfulImportDate]     DATETIME2        NOT NULL,
    [LastAttemptedDate]            DATETIME2        NOT NULL,
    [LastAttemptSuccessful]        BIT              NOT NULL    DEFAULT 0,
    [TotalRecordsLastRun]          INT              NOT NULL    DEFAULT 0,
    [FailedRecordsLastRun]         INT              NOT NULL    DEFAULT 0,
    [UpdatedDate]                  DATETIME2        NOT NULL    DEFAULT GETUTCDATE(),
    [CreatedDate]                  DATETIME2        NOT NULL    DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ImportJobState]
        PRIMARY KEY CLUSTERED ([Id]),

    -- Scoped to account: two accounts can share a job name
    CONSTRAINT [UQ_ImportJobState_AccountId_JobName]
        UNIQUE ([AccountId], [JobName]),

    CONSTRAINT [CK_JobName_NotEmpty]
        CHECK (LEN([JobName]) > 0)
)
GO

CREATE NONCLUSTERED INDEX [IX_ImportJobState_AccountId]
    ON [dbo].[ImportJobState] ([AccountId])
    INCLUDE ([JobName], [LastSuccessfulImportDate], [LastAttemptSuccessful])
GO

-- The unique constraint already creates an index on (AccountId, JobName),
-- so a separate IX on JobName alone would be redundant — removed.
CREATE NONCLUSTERED INDEX [IX_ImportJobState_LastAttempt]
    ON [dbo].[ImportJobState] ([LastAttemptedDate] DESC)
GO