/***********************************************************************************************
	DosMail.cpp

	Usage:
		DosMail server from_address to_address [subject] message
/**********************************************************************************************/

#define _WIN32_WINNT  0x0502
#include <windows.h>
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>

#define ERROR_HOST          "Could not find the host."
#define ERROR_SOCKET        "Could not create a socket."
#define ERROR_CONNECT       "Could not connect to the socket."
#define ERROR_READ          "Could not read from the socket."
#define ERROR_WRITE         "Could not write to the socket."
#define ERROR_NO_FROM       "The \"From\" address is not valid."
#define ERROR_NO_RECIPIENTS "The \"To\" address is not valid."
#define ERROR_NO_MESSAGE    "The message could not be sent."
#define ERROR_NO_MEMORY     "There is not enough memory."

int Error(char * pszError, SOCKET s)
{
	printf("\nError: %s\n", pszError);
	if (s)
		closesocket(s);
	WSACleanup();
	return 1;
}

SOCKET s = 0;

int _send(char * pszBuffer, int cchBuffer)
{
	printf("%s\n", pszBuffer);
	return send(s, pszBuffer, strlen(pszBuffer), 0);
}

int _recv(char * pszBuffer, int cchBuffer)
{
	memset(pszBuffer, 0, cchBuffer);
	int _ret = recv(s, pszBuffer, cchBuffer, 0);
	printf("%s\n", pszBuffer);
	return _ret;
}

int SendMessage(char * pszBuffer, int cchBuffer)
{
	if (_send(pszBuffer, strlen(pszBuffer)) == INVALID_SOCKET)
		return Error(ERROR_WRITE, s);
	memset(pszBuffer, 0, cchBuffer);
	if (_recv(pszBuffer, cchBuffer) == INVALID_SOCKET)
		return Error(ERROR_READ, s);
	return atoi(pszBuffer);
}

int main(int argc, char ** argv)
{
	if (argc < 5 || argc > 6)
	{
		printf("Usage: DosMail server from_address to_address [subject] message\n");
		return 1;
	}

	char * pszServer = argv[1];
	char * pszFromAddress = argv[2];
	char * pszToAddress = argv[3];
	char * pszSubject = NULL;
	char * pszMessage = NULL;
	if (argc == 5)
	{
		pszMessage = argv[4];
	}
	else
	{
		pszSubject = argv[4];
		pszMessage = argv[5];
	}

	struct hostent * hp;
	struct sockaddr_in sin = {0};
	const int kcchBuffer = 200;
	char szBuffer[kcchBuffer];
	WSADATA wsaData;

	if (WSAStartup(MAKEWORD(2, 0), &wsaData))
	{
		printf("\nError: Could not find the WinSock DLL\n.");
		return 1;
	}
	if ((hp = gethostbyname(pszServer)) == 0)
		return Error(ERROR_HOST, s);

   // Make a connection to the host on port 25
	memcpy(&sin.sin_addr, hp->h_addr, hp->h_length);
	sin.sin_family = AF_INET;
	//sin.sin_family = hp->h_addrtype;
	sin.sin_port = htons(25);
	if ((s = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET)
		return Error(ERROR_SOCKET, s);
	if (connect(s, (struct sockaddr *) &sin, sizeof(sin)) == INVALID_SOCKET)
		return Error(ERROR_CONNECT, s);
	if (_recv(szBuffer, sizeof(szBuffer)) == INVALID_SOCKET)
		return Error(ERROR_READ, s);

	// Send helo message
	DWORD cchHost = 200;
	char szHost[200];
	if (!GetComputerNameEx(ComputerNameDnsFullyQualified, szHost, &cchHost))
		strcpy_s(szHost, cchHost, pszServer);
	sprintf_s(szBuffer, kcchBuffer, "helo %s\n", szHost);
	if (SendMessage(szBuffer, kcchBuffer) != 250)
		return Error(ERROR_CONNECT, s);

	// Send "mail from:<>" message
	sprintf_s(szBuffer, kcchBuffer, "mail from:<%s>\n", pszFromAddress);
	if (SendMessage(szBuffer, kcchBuffer) != 250)
		return Error(ERROR_NO_FROM, s);

	// Send "rcpt to:<>" message(s)
	sprintf_s(szBuffer, kcchBuffer, "rcpt to:<%s>\n", pszToAddress);
	if (SendMessage(szBuffer, kcchBuffer) != 250)
		return Error(ERROR_NO_RECIPIENTS, s);

	// Send "data" message
	strcpy_s(szBuffer, kcchBuffer, "data\n");
	if (SendMessage(szBuffer, kcchBuffer) != 354)
		return Error(ERROR_NO_MESSAGE, s);

	// Send subject line
	if (pszSubject)
	{
		sprintf_s(szBuffer, kcchBuffer, "Subject: %s\n", pszSubject);
		if (_send(szBuffer, strlen(szBuffer)) == INVALID_SOCKET)
			return Error(ERROR_WRITE, s);
	}

	// Send "to" line
	if (pszSubject)
	{
		sprintf_s(szBuffer, kcchBuffer, "To: %s\n", pszToAddress);
		if (_send(szBuffer, strlen(szBuffer)) == INVALID_SOCKET)
			return Error(ERROR_WRITE, s);
	}

	// Send message lines
	sprintf_s(szBuffer, kcchBuffer, "%s\n", pszMessage);
	if (strcmp(szBuffer, ".\n") == 0)
		strcpy_s(szBuffer, kcchBuffer, "..\n");
	if (_send(szBuffer, strlen(szBuffer)) == INVALID_SOCKET)
		return Error(ERROR_WRITE, s);

	// Send time stamp
	time_t ltime;
	time(&ltime);
	const int cchTime = 26;
	char szTime[cchTime];
	ctime_s(szTime, cchTime, &ltime);
	sprintf_s(szBuffer, kcchBuffer, "Time sent: %s", szTime);
	if (strcmp(szBuffer, ".\n") == 0)
		strcpy_s(szBuffer, kcchBuffer, "..\n");
	if (_send(szBuffer, strlen(szBuffer)) == INVALID_SOCKET)
		return Error(ERROR_WRITE, s);

	// End the message
	strcpy_s(szBuffer, kcchBuffer, ".\r\n");
	if (SendMessage(szBuffer, kcchBuffer) != 250)
		return Error(ERROR_NO_MESSAGE, s);

	// Send "quit" message and close socket
	strcpy_s(szBuffer, kcchBuffer, "quit\n");
	_send(szBuffer, strlen(szBuffer));
	closesocket(s);
	s = 0;

	// Print out success or failure of message
	printf("\nThe message was sent successfully.\n");
	WSACleanup();
	return 0;
}