using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rp.GoogleSheet
{
    public static class GoogleSheetUtil
    {
        public static string GetSheetLink(string sheetKey) => "https://docs.google.com/spreadsheets/d/" + sheetKey + "/export?format=tsv";
    }
}
