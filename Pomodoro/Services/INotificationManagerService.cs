﻿using Pomodoro.Entities;

public interface INotificationManagerService
{
    event EventHandler NotificationReceived;
    void SendNotification(string title, string message, PomodoroTimer pomodoroTimer, DateTime? notifyTime = null);
    void ReceiveNotification(string title, string message);
    void DeleteCurrentNotification();
}