using Android.Content;
using Android.Media;
using Pomodoro.Entities;
using NetUri = Android.Net.Uri;
using AndroidApp = Android.App.Application;
using Pomodoro.Services;
namespace Pomodoro.Platforms.Android;

public class MyRingtoneManager : IRingtoneManagerService
{
    private MediaPlayer mediaPlayer;
    public void PlayCurrentNotificationSound()
    {
        NetUri uri = PomodoroTimer.Instance.IsDefaultSound ? RingtoneManager.GetDefaultUri(RingtoneType.Notification) : NetUri.Parse($"{ContentResolver.SchemeAndroidResource}://{AndroidApp.Context.PackageName}/raw/custom_sound");
        if (mediaPlayer != null)
        {
            mediaPlayer.Stop();
            mediaPlayer.Release();
            mediaPlayer = null;
        }

        mediaPlayer = MediaPlayer.Create(AndroidApp.Context, uri);

        if (mediaPlayer != null)
        {
            mediaPlayer.Start();
        }
    }
}
