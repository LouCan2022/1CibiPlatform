window.downloadExcel = (fileName, jsonData) => {
    const ws = XLSX.utils.json_to_sheet(jsonData);

    // Make header bold
    const range = XLSX.utils.decode_range(ws['!ref']);
    for (let C = range.s.c; C <= range.e.c; ++C) {
        const cell_address = XLSX.utils.encode_cell({ c: C, r: 0 });
        if (!ws[cell_address]) continue;
        ws[cell_address].s = { font: { bold: true } };
    }

    // Auto-fit columns based on header + data
    const cols = Object.keys(jsonData[0]).map((key, colIndex) => {
        let maxLength = key.length;
        for (let row = 0; row < jsonData.length; row++) {
            const value = jsonData[row][key];
            if (value != null) {
                maxLength = Math.max(maxLength, value.toString().length);
            }
        }
        return { wch: maxLength + 2 }; // +2 for padding
    });

    ws['!cols'] = cols;

    // Create workbook
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Sheet1");

    XLSX.writeFile(wb, fileName);
};
