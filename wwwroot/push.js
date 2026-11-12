window.walterPush = {
  async enable(publicKey) {
    if (!('serviceWorker' in navigator) || !('PushManager' in window) || !('Notification' in window)) return 'Push notifications are not supported in this browser.';
    if (!window.isSecureContext) return 'Push notifications require HTTPS or localhost.';
    try {
      const permission = await Notification.requestPermission();
      if (permission !== 'granted') return 'Notification permission was not granted.';
      const registration = await navigator.serviceWorker.register('/service-worker.js');
      const ready = await navigator.serviceWorker.ready;
      let subscription = await ready.pushManager.getSubscription();
      if (!subscription) subscription = await ready.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: this.decodeKey(publicKey) });
      const response = await fetch('/api/push/subscribe', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(subscription.toJSON()) });
      if (!response.ok) return 'Could not save the push subscription.';
      return 'Push reminders enabled on this device.';
    } catch (error) { return 'Could not enable push reminders: ' + error.message; }
  },
  decodeKey(key) {
    const padded = key + '='.repeat((4 - key.length % 4) % 4);
    const binary = atob(padded.replace(/-/g, '+').replace(/_/g, '/'));
    return Uint8Array.from(binary, c => c.charCodeAt(0));
  },
  async disable() {
    const registration = await navigator.serviceWorker.getRegistration('/service-worker.js');
    const subscription = await registration?.pushManager.getSubscription();
    if (!subscription) return 'Push reminders are already disabled.';
    const endpoint = subscription.endpoint;
    await subscription.unsubscribe();
    await fetch('/api/push/unsubscribe', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ endpoint }) });
    return 'Push reminders disabled on this device.';
  }
};