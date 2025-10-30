window.startLivenessInterop = (HashToken, dotNetHelper) => {
    if (!window.eKYC) {
        console.error("❌ eKYC SDK not loaded!");
        alert("Liveness SDK not loaded properly. Check script source.");
        return;
    }
    window.eKYC().start({
        pubKey: "eyJpdiI6Im9YTTRTTXpwbDF0ZlRvakFHRG1HTnc9PSIsInZhbHVlIjoiUlo3WFJmM1dZUEVSdmNNbDJrU3o2Zz09IiwibWFjIjoiZjJmNWQxN2M4ZjgxMDQ1NDE5MzYzNTU1ZWNiMzU0MDk3Y2ZkNjc5NDA1Y2VlOTViOTQ5NmJhMWIzN2NiMzIxZCIsInRhZyI6IiJ9"
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