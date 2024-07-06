﻿using Pomodoro.Entities;

public interface INotificationManagerService
{
    event EventHandler NotificationReceived;
    void SendNotification(string title, string message, DateTime? notifyTime = null, PomodoroTimer? pomodoroTimer = null);
    void ReceiveNotification(string title, string message);
    void DeleteCurrentNotification();
}