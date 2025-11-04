window.startLivenessInterop = (HashToken, dotNetHelper, livenessKey) => {
    if (!window.eKYC) {
        console.error("❌ eKYC SDK not loaded!");
        alert("Liveness SDK not loaded properly. Check script source.");
        return;
    }
    window.eKYC().start({
        pubKey: livenessKey
    }).then((data) => {
        if (data && data.result && data.result.session_id) {
            const sessionId = data.result.session_id;

            console.log("✅ Liveness Check Completed for Token:", HashToken);

            dotNetHelper.invokeMethodAsync('OnLivenessCompleted', sessionId);

        } else {
            console.warn("⚠️ No session_id found in SDK result:", data);
        }
    }).catch((err) => {
        console.error("❌ Liveness failed:", err);
    });
};