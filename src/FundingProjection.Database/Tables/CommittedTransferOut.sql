CREATE TABLE dbo.[CommittedTransferOut]
(
    [Id]                        UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [TransferSenderId]          BIGINT              NOT NULL,   
    [ApprenticeshipId]          BIGINT              NOT NULL,
    [PledgeApplicationId]       INT                 NULL,       
    [TransferApprovalStatus]    TINYINT             NOT NULL,   -- Commitment.TransferApprovalStatus: 0 = Pending, 1 = Approved, 2 = Rejected
    [RemainingValue]            DECIMAL(18, 2)      NOT NULL,
    [PlannedEndDate]            DATE                NOT NULL,
    [Status]                    NVARCHAR(50)        NOT NULL,   -- 'Active', 'Completed', 'Stopped'
    [LastUpdatedDate]           DATETIME2           NOT NULL,

    CONSTRAINT [PK_CommittedTransferOut]
        PRIMARY KEY CLUSTERED ([Id]),

    CONSTRAINT [UQ_CommittedTransferOut_Sender_Apprenticeship]
        UNIQUE ([TransferSenderId], [ApprenticeshipId])
)

GO

CREATE NONCLUSTERED INDEX [IX_CommittedTransferOut_TransferSenderId]
    ON [dbo].[CommittedTransferOut] ([TransferSenderId])
    INCLUDE ([RemainingValue], [TransferApprovalStatus], [PledgeApplicationId], [Status])