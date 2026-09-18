CREATE TABLE dbo.[ImportJobState]
(
    [Id]                           UniqueIdentifier NOT NULL DEFAULT NEWSEQUENTIALID(),
    [JobName]                      NVARCHAR(100)   NOT NULL,
    [LastSuccessfulImportDate]     DATETIME2       NOT NULL,
    [LastAttemptedDate]            DATETIME2       NOT NULL,
    [LastAttemptSuccessful]        BIT             NOT NULL    DEFAULT 0,
    [TotalRecordsLastRun]          INT             NOT NULL    DEFAULT 0,
    [FailedRecordsLastRun]         INT             NOT NULL    DEFAULT 0,
    [UpdatedDate]                  DATETIME2       NOT NULL    DEFAULT GETUTCDATE(),
    [CreatedDate]                  DATETIME2       NOT NULL    DEFAULT GETUTCDATE(),

    CONSTRAINT [PK_ImportJobState]
PRIMARY KEY CLUSTERED ([Id]),

CONSTRAINT [UQ_ImportJobState_JobName]
UNIQUE ([JobName]),

CONSTRAINT [CK_JobName_NotEmpty]
CHECK (LEN([JobName]) > 0)
    )
GO

    -- Create indexes for common queries
CREATE NONCLUSTERED INDEX [IX_ImportJobState_JobName]
ON [dbo].[ImportJobState] ([JobName])
INCLUDE ([LastSuccessfulImportDate], [LastAttemptSuccessful])
GO

    CREATE NONCLUSTERED INDEX [IX_ImportJobState_LastAttempt]
ON [dbo].[ImportJobState] ([LastAttemptedDate] DESC)
GO