#if UNITY_IOS
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace DigitsNFCToolkit.Editor
{
	public class PostProcessBuild
	{
		public static readonly string[] ISO7816_IDENTIFIERS = new string[]
		{
			"A0000002471001",
			"A000000003101001",
			"A0000000042010",
			"A0000000044010",
			"44464D46412E44466172653234313031",
			"D2760000850100",
			"D2760000850101",
			"00000000000000",
			"F00000ABBA0103",
			"D2760000850101h",
			"315041592E5359532E4444463031",
			"D4100000030001",
			"325041592E5359532E4444463031",
			"44464D46412E44466172653234313031",
			"A00000000101",
			"A000000003000000",
			"A00000000300037561",
			"A00000000305076010",
			"A0000000031010",
			"A000000003101001",
			"A000000003101002",
			"A0000000032010",
			"A0000000032020",
			"A0000000033010",
			"A0000000034010",
			"A0000000035010",
			"A000000003534441",
			"A0000000035350",
			"A000000003535041",
			"A0000000036010",
			"A0000000036020",
			"A0000000038002",
			"A0000000038010",
			"A0000000039010",
			"A000000003999910",
			"A0000000040000",
			"A00000000401",
			"A0000000041010",
			"A00000000410101213",
			"A00000000410101215",
			"A0000000041010BB5449435301",
			"A0000000042010",
			"A0000000042203",
			"A0000000043010",
			"A0000000043060",
			"A000000004306001",
			"A0000000044010",
			"A0000000045010",
			"A0000000045555",
			"A0000000046000",
			"A0000000048002",
			"A0000000049999",
			"A0000000050001",
			"A0000000050002",
			"A0000000090001FF44FF1289",
			"A0000000101030",
			"A00000001800",
			"A0000000181001",
			"A000000018434D",
			"A000000018434D00",
			"A00000002401",
			"A000000025",
			"A0000000250000",
			"A00000002501",
			"A000000025010104",
			"A000000025010402",
			"A000000025010701",
			"A000000025010801",
			"A0000000291010",
			"A00000002945087510100000",
			"A00000002949034010100001",
			"A00000002949282010100000",
			"A000000029564182",
			"A00000003029057000AD13100101FF",
			"A0000000308000000000280101",
			"A0000000421010",
			"A0000000422010",
			"A0000000423010",
			"A0000000424010",
			"A0000000425010",
			"A0000000426010",
			"A00000005945430100",
			"A000000063504B43532D3135",
			"A0000000635741502D57494D",
			"A00000006510",
			"A0000000651010",
			"A00000006900",
			"A000000077010000021000000000003B",
			"A0000000790100",
			"A0000000790101",
			"A0000000790102",
			"A00000007901F0",
			"A00000007901F1",
			"A00000007901F2",
			"A0000000790200",
			"A0000000790201",
			"A00000007902FB",
			"A00000007902FD",
			"A00000007902FE",
			"A0000000790300",
			"A0000000791201",
			"A0000000791202",
			"A0000000871002FF49FF0589",
			"A00000008810200105C100",
			"A000000088102201034221",
			"A000000088102201034321",
			"A0000000960200",
			"A000000098",
			"A0000000980840",
			"A0000000980848",
			"A0000001110101",
			"A0000001110201",
			"A0000001160300",
			"A0000001166010",
			"A0000001166030",
			"A0000001169000",
			"A000000116A001",
			"A000000116DB00",
			"A000000118010000",
			"A000000118020000",
			"A000000118030000",
			"A000000118040000",
			"A0000001184543",
			"A000000118454E",
			"A0000001211010",
			"A0000001320001",
			"A0000001408001",
			"A0000001410001",
			"A0000001510000",
			"A00000015153504341534400",
			"A0000001523010",
			"A0000001524010",
			"A0000001544442",
			"A0000001570010",
			"A0000001570020",
			"A0000001570021",
			"A0000001570022",
			"A0000001570023",
			"A0000001570030",
			"A0000001570031",
			"A0000001570040",
			"A0000001570050",
			"A0000001570051",
			"A0000001570100",
			"A0000001570104",
			"A0000001570109",
			"A000000157010A",
			"A000000157010B",
			"A000000157010C",
			"A000000157010D",
			"A0000001574443",
			"A0000001574444",
			"A000000167413000FF",
			"A000000167413001",
			"A000000172950001",
			"A000000177504B43532D3135",
			"A0000001850002",
			"A0000001884443",
			"A0000002040000",
			"A0000002281010",
			"A0000002282010",
			"A00000022820101010",
			"A0000002471001",
			"A0000002472001",
			"A0000002771010",
			"A00000030600000000000000",
			"A000000308000010000100",
			"A00000031510100528",
			"A0000003156020",
			"A00000032301",
			"A0000003230101",
			"A0000003241010",
			"A000000333010101",
			"A000000333010102",
			"A000000333010103",
			"A000000333010106",
			"A000000333010108",
			"A000000337301000",
			"A000000337101000",
			"A000000337102000",
			"A000000337101001",
			"A000000337102001",
			"A000000337601001",
			"A0000003591010",
			"A0000003591010028001",
			"A00000035910100380",
			"A0000003660001",
			"A0000003660002",
			"A0000003710001",
			"A00000038410",
			"A00000038420",
			"A0000003964D66344D0002",
			"A00000039742544659",
			"A0000003974349445F0100",
			"A0000004271010",
			"A0000004320001",
			"A0000004360100",
			"A0000004391010",
			"A0000004540010",
			"A0000004540011",
			"A0000004762010",
			"A0000004763030",
			"A0000004766C",
			"A000000476A010",
			"A000000476A110",
			"A000000485",
			"A0000005241010",
			"A0000005271002",
			"A000000527200101",
			"A000000527210101",
			"A0000005591010FFFFFFFF8900000100",
			"A0000005591010FFFFFFFF8900000200",
			"A0000005591010FFFFFFFF8900000D00",
			"A0000005591010FFFFFFFF8900000E00",
			"A0000005591010FFFFFFFF8900000F00",
			"A0000005591010FFFFFFFF8900001000",
			"A00000061700",
			"A0000006200620",
			"A0000006260101010001",
			"A0000006351010",
			"A0000006581010",
			"A0000006581011",
			"A0000006582010",
			"A0000006723010",
			"A0000006723020",
			"A0000007705850",
			"A0000007790000",
			"B012345678",
			"D040000001000002",
			"D040000002000002",
			"D040000003000002",
			"D040000004000002",
			"D04000000B000002",
			"D04000000C000002",
			"D04000000D000002",
			"D040000013000001",
			"D040000013000001",
			"D040000013000002",
			"D040000013000002",
			"D040000014000001",
			"D040000015000001",
			"D040000015000001",
			"D0400000190001",
			"D0400000190002",
			"D0400000190003",
			"D0400000190004",
			"D0400000190010",
			"D268000001",
			"D276000005",
			"D276000005AA040360010410",
			"D276000005AA0503E00401",
			"D276000005AA0503E00501",
			"D276000005AA0503E0050101",
			"D276000005AB0503E0040101",
			"D27600002200000001",
			"D27600002200000002",
			"D27600002200000060",
			"D276000025",
			"D27600002545410100",
			"D27600002545500100",
			"D27600002547410100",
			"D276000060",
			"D2760000850100",
			"D2760000850101",
			"D276000118",
			"D2760001180101",
			"D27600012401",
			"D276000124010101FFFF000000010000",
			"D2760001240102000000000000010000",
			"D27600012402",
			"D2760001240200010000000000000000",
			"D4100000011010",
			"D5280050218002",
			"D5780000021010",
			"D7560000010101",
			"D7560000300101",
			"D8040000013010",
			"E80704007F00070302",
			"E82881C11702",
			"E828BD080F",
			"F0000000030001"
		};

#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                AddNFCReaderUsageDescription(path);
                AddNFCCapability(path);
            }
        }

        private static void AddNFCReaderUsageDescription(string path)
        {
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetString("NFCReaderUsageDescription", "Reading NFC Tags");

            PlistElementArray felicaSystemCodes = rootDict.CreateArray("com.apple.developer.nfc.readersession.felica.systemcodes");
            felicaSystemCodes.AddString("12FC");

            PlistElementArray iso7816SelectIdentifiers = rootDict.CreateArray("com.apple.developer.nfc.readersession.iso7816.select-identifiers");
            int length = ISO7816_IDENTIFIERS.Length;
            for(int i = 0; i < length; i++)
            {
                iso7816SelectIdentifiers.AddString(ISO7816_IDENTIFIERS[i]);
            }

            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void AddNFCCapability(string path)
        {
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string packageName = UnityEngine.Application.identifier;
            string name = packageName.Substring(packageName.LastIndexOf('.') + 1);
            string entitlementFileName = name + ".entitlements";
            string entitlementPath = Path.Combine(path, entitlementFileName);

#if UNITY_2019_3_OR_NEWER
            ProjectCapabilityManager projectCapabilityManager = new ProjectCapabilityManager(projectPath, entitlementFileName, null, project.GetUnityMainTargetGuid());
#else
            ProjectCapabilityManager projectCapabilityManager = new ProjectCapabilityManager(projectPath, entitlementFileName, PBXProject.GetUnityTargetName());
#endif
            PlistDocument entitlementDocument = AddNFCEntitlement(projectCapabilityManager);
            entitlementDocument.WriteToFile(entitlementPath);

            var projectInfo = projectCapabilityManager.GetType().GetField("project", BindingFlags.NonPublic | BindingFlags.Instance);
            project = (PBXProject)projectInfo.GetValue(projectCapabilityManager);

#if UNITY_2019_3_OR_NEWER
            string target = project.GetUnityMainTargetGuid();
#else
			string target = project.TargetGuidByName(PBXProject.GetUnityTargetName()); //Use this line for older versions of Unity
#endif

#if UNITY_2022_3_OR_NEWER
            PBXCapabilityType nfcCapability = new PBXCapabilityType("com.apple.NearFieldCommunicationTagReading", true, "", false);
#else
            var constructor = typeof(PBXCapabilityType).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[]{typeof(string), typeof(bool), typeof(string), typeof(bool)}, null);
            PBXCapabilityType nfcCapability = (PBXCapabilityType)constructor.Invoke(new object[] { "com.apple.NearFieldCommunicationTagReading", true, "", false });
#endif
            project.AddCapability(target, nfcCapability, entitlementFileName);
            project.AddFrameworkToProject(target, "CoreNFC.framework", true);

            projectCapabilityManager.WriteToFile();
        }

        private static PlistDocument AddNFCEntitlement(ProjectCapabilityManager projectCapabilityManager)
        {
            MethodInfo getMethod = projectCapabilityManager.GetType().GetMethod("GetOrCreateEntitlementDoc", BindingFlags.NonPublic | BindingFlags.Instance);
            PlistDocument entitlementDoc = (PlistDocument)getMethod.Invoke(projectCapabilityManager, new object[] { });

            PlistElementDict dictionary = entitlementDoc.root;
            PlistElementArray array = dictionary.CreateArray("com.apple.developer.nfc.readersession.formats");
            array.values.Add(new PlistElementString("TAG"));
            array.values.Add(new PlistElementString("NDEF"));

            return entitlementDoc;
        }
#endif
	}
}

