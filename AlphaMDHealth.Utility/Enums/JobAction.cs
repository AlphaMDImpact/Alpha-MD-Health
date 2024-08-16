namespace AlphaMDHealth.Utility;

public enum JobAction
{
	Default = 0,
	UpdateTaskStatus = 1,
	PendingCommunication = 2,
	ArchiveErrorLogs = 3,
	ArchiveAuditLogs = 4,
	ArchiveUserAccountSessionsHistory = 5,
	ArchiveUserCommunicationsHistory = 6
}