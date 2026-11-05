namespace SFA.DAS.FundingProjection.Domain.Enums;

// These are the only types that are ever imported into finance that make up the cost of an apprenticeship
public enum TransactionType
{
    Learning = 1,
    Completion = 2,
    Balancing = 3,
    Milestone1 = 17 //only for apprenticeship units
}
