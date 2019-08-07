#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Haegin.Resolvers
{
    using System;
    using MessagePack;

    public class HaeginResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new HaeginResolver();

        HaeginResolver()
        {

        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                var f = HaeginResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class HaeginResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static HaeginResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(84)
            {
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.StoreKitTransaction>), 0 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.GooglePurchasedProduct>), 1 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.GoogleConsumedProduct>), 2 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.EventItem>), 3 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.TermsKindVersion>), 4 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.Terms>), 5 },
                {typeof(global::System.Collections.Generic.List<string>), 6 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.ShopProductInfo>), 7 },
                {typeof(global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>), 8 },
                {typeof(global::HaeginGame.ProtocolId), 9 },
                {typeof(global::Haegin.BannerType), 10 },
                {typeof(global::Haegin.Result), 11 },
                {typeof(global::Haegin.MarketType), 12 },
                {typeof(global::Haegin.OsType), 13 },
                {typeof(global::Haegin.AccountType), 14 },
                {typeof(global::Haegin.BlockType), 15 },
                {typeof(global::Haegin.LinkOption), 16 },
                {typeof(global::HaeginGame.TransactionState), 17 },
                {typeof(global::HaeginGame.ConsumptionState), 18 },
                {typeof(global::Haegin.Language), 19 },
                {typeof(global::HaeginGame.Protocol), 20 },
                {typeof(global::HaeginGame.ProtocolRes), 21 },
                {typeof(global::Haegin.Player), 22 },
                {typeof(global::Haegin.PurchasedInfo), 23 },
                {typeof(global::HaeginGame.AliveReq), 24 },
                {typeof(global::HaeginGame.AliveRes), 25 },
                {typeof(global::HaeginGame.ApnsRegisterReq), 26 },
                {typeof(global::HaeginGame.ApnsRegisterRes), 27 },
                {typeof(global::HaeginGame.AuthReq), 28 },
                {typeof(global::HaeginGame.AuthRes), 29 },
                {typeof(global::HaeginGame.GameCenterSignature), 30 },
                {typeof(global::HaeginGame.AuthAppleReq), 31 },
                {typeof(global::HaeginGame.AuthAppleRes), 32 },
                {typeof(global::HaeginGame.AuthFacebookReq), 33 },
                {typeof(global::HaeginGame.AuthFacebookRes), 34 },
                {typeof(global::HaeginGame.AuthGoogleReq), 35 },
                {typeof(global::HaeginGame.AuthGoogleRes), 36 },
                {typeof(global::HaeginGame.ConsumeAppleReceiptReq), 37 },
                {typeof(global::HaeginGame.StoreKitTransaction), 38 },
                {typeof(global::HaeginGame.ConsumeAppleReceiptRes), 39 },
                {typeof(global::HaeginGame.GooglePurchasedProduct), 40 },
                {typeof(global::HaeginGame.ConsumeGoogleReceiptReq), 41 },
                {typeof(global::HaeginGame.GoogleConsumedProduct), 42 },
                {typeof(global::HaeginGame.ConsumeGoogleReceiptRes), 43 },
                {typeof(global::HaeginGame.ErrorRes), 44 },
                {typeof(global::HaeginGame.EventListReq), 45 },
                {typeof(global::HaeginGame.EventItem), 46 },
                {typeof(global::HaeginGame.EventListRes), 47 },
                {typeof(global::HaeginGame.FcmRegisterReq), 48 },
                {typeof(global::HaeginGame.FcmRegisterRes), 49 },
                {typeof(global::HaeginGame.FcmUnregisterReq), 50 },
                {typeof(global::HaeginGame.FcmUnregisterRes), 51 },
                {typeof(global::HaeginGame.HandshakeReq), 52 },
                {typeof(global::HaeginGame.HandshakeRes), 53 },
                {typeof(global::HaeginGame.ProtocolReq), 54 },
                {typeof(global::HaeginGame.TermsKindVersion), 55 },
                {typeof(global::HaeginGame.TermsConfirmReq), 56 },
                {typeof(global::HaeginGame.TermsConfirmRes), 57 },
                {typeof(global::HaeginGame.TermsListReq), 58 },
                {typeof(global::HaeginGame.Terms), 59 },
                {typeof(global::HaeginGame.TermsListRes), 60 },
                {typeof(global::HaeginGame.LineItem), 61 },
                {typeof(global::HaeginGame.AuthSteamReq), 62 },
                {typeof(global::HaeginGame.AuthSteamRes), 63 },
                {typeof(global::HaeginGame.SteamFinalizeTransactionReq), 64 },
                {typeof(global::HaeginGame.SteamFinalizeTransactionRes), 65 },
                {typeof(global::HaeginGame.SteamInitTransactionReq), 66 },
                {typeof(global::HaeginGame.SteamInitTransactionRes), 67 },
                {typeof(global::HaeginGame.ShopProductListReq), 68 },
                {typeof(global::HaeginGame.ShopProductInfo), 69 },
                {typeof(global::HaeginGame.ShopProductListRes), 70 },
                {typeof(global::HaeginGame.MaintenanceCheckReq), 71 },
                {typeof(global::HaeginGame.MaintenanceCheckRes), 72 },
                {typeof(global::HaeginGame.NoticeReq), 73 },
                {typeof(global::HaeginGame.NoticeRes), 74 },
                {typeof(global::HaeginGame.KeyCountReq), 75 },
                {typeof(global::HaeginGame.KeyCountRes), 76 },
                {typeof(global::HaeginGame.CouponReq), 77 },
                {typeof(global::HaeginGame.CouponRes), 78 },
                {typeof(global::HaeginGame.MaintenanceCheckV2Req), 79 },
                {typeof(global::HaeginGame.MaintenanceCheckV2Res), 80 },
                {typeof(global::HaeginGame.OneStorePurchasedProduct), 81 },
                {typeof(global::HaeginGame.ConsumeOneStoreReceiptReq), 82 },
                {typeof(global::HaeginGame.ConsumeOneStoreReceiptRes), 83 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.StoreKitTransaction>();
                case 1: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.GooglePurchasedProduct>();
                case 2: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.GoogleConsumedProduct>();
                case 3: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.EventItem>();
                case 4: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.TermsKindVersion>();
                case 5: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.Terms>();
                case 6: return new global::MessagePack.Formatters.ListFormatter<string>();
                case 7: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.ShopProductInfo>();
                case 8: return new global::MessagePack.Formatters.ListFormatter<global::HaeginGame.OneStorePurchasedProduct>();
                case 9: return new Haegin.Formatters.HaeginGame.ProtocolIdFormatter();
                case 10: return new Haegin.Formatters.Haegin.BannerTypeFormatter();
                case 11: return new Haegin.Formatters.Haegin.ResultFormatter();
                case 12: return new Haegin.Formatters.Haegin.MarketTypeFormatter();
                case 13: return new Haegin.Formatters.Haegin.OsTypeFormatter();
                case 14: return new Haegin.Formatters.Haegin.AccountTypeFormatter();
                case 15: return new Haegin.Formatters.Haegin.BlockTypeFormatter();
                case 16: return new Haegin.Formatters.Haegin.LinkOptionFormatter();
                case 17: return new Haegin.Formatters.HaeginGame.TransactionStateFormatter();
                case 18: return new Haegin.Formatters.HaeginGame.ConsumptionStateFormatter();
                case 19: return new Haegin.Formatters.Haegin.LanguageFormatter();
                case 20: return new Haegin.Formatters.HaeginGame.ProtocolFormatter();
                case 21: return new Haegin.Formatters.HaeginGame.ProtocolResFormatter();
                case 22: return new Haegin.Formatters.Haegin.PlayerFormatter();
                case 23: return new Haegin.Formatters.Haegin.PurchasedInfoFormatter();
                case 24: return new Haegin.Formatters.HaeginGame.AliveReqFormatter();
                case 25: return new Haegin.Formatters.HaeginGame.AliveResFormatter();
                case 26: return new Haegin.Formatters.HaeginGame.ApnsRegisterReqFormatter();
                case 27: return new Haegin.Formatters.HaeginGame.ApnsRegisterResFormatter();
                case 28: return new Haegin.Formatters.HaeginGame.AuthReqFormatter();
                case 29: return new Haegin.Formatters.HaeginGame.AuthResFormatter();
                case 30: return new Haegin.Formatters.HaeginGame.GameCenterSignatureFormatter();
                case 31: return new Haegin.Formatters.HaeginGame.AuthAppleReqFormatter();
                case 32: return new Haegin.Formatters.HaeginGame.AuthAppleResFormatter();
                case 33: return new Haegin.Formatters.HaeginGame.AuthFacebookReqFormatter();
                case 34: return new Haegin.Formatters.HaeginGame.AuthFacebookResFormatter();
                case 35: return new Haegin.Formatters.HaeginGame.AuthGoogleReqFormatter();
                case 36: return new Haegin.Formatters.HaeginGame.AuthGoogleResFormatter();
                case 37: return new Haegin.Formatters.HaeginGame.ConsumeAppleReceiptReqFormatter();
                case 38: return new Haegin.Formatters.HaeginGame.StoreKitTransactionFormatter();
                case 39: return new Haegin.Formatters.HaeginGame.ConsumeAppleReceiptResFormatter();
                case 40: return new Haegin.Formatters.HaeginGame.GooglePurchasedProductFormatter();
                case 41: return new Haegin.Formatters.HaeginGame.ConsumeGoogleReceiptReqFormatter();
                case 42: return new Haegin.Formatters.HaeginGame.GoogleConsumedProductFormatter();
                case 43: return new Haegin.Formatters.HaeginGame.ConsumeGoogleReceiptResFormatter();
                case 44: return new Haegin.Formatters.HaeginGame.ErrorResFormatter();
                case 45: return new Haegin.Formatters.HaeginGame.EventListReqFormatter();
                case 46: return new Haegin.Formatters.HaeginGame.EventItemFormatter();
                case 47: return new Haegin.Formatters.HaeginGame.EventListResFormatter();
                case 48: return new Haegin.Formatters.HaeginGame.FcmRegisterReqFormatter();
                case 49: return new Haegin.Formatters.HaeginGame.FcmRegisterResFormatter();
                case 50: return new Haegin.Formatters.HaeginGame.FcmUnregisterReqFormatter();
                case 51: return new Haegin.Formatters.HaeginGame.FcmUnregisterResFormatter();
                case 52: return new Haegin.Formatters.HaeginGame.HandshakeReqFormatter();
                case 53: return new Haegin.Formatters.HaeginGame.HandshakeResFormatter();
                case 54: return new Haegin.Formatters.HaeginGame.ProtocolReqFormatter();
                case 55: return new Haegin.Formatters.HaeginGame.TermsKindVersionFormatter();
                case 56: return new Haegin.Formatters.HaeginGame.TermsConfirmReqFormatter();
                case 57: return new Haegin.Formatters.HaeginGame.TermsConfirmResFormatter();
                case 58: return new Haegin.Formatters.HaeginGame.TermsListReqFormatter();
                case 59: return new Haegin.Formatters.HaeginGame.TermsFormatter();
                case 60: return new Haegin.Formatters.HaeginGame.TermsListResFormatter();
                case 61: return new Haegin.Formatters.HaeginGame.LineItemFormatter();
                case 62: return new Haegin.Formatters.HaeginGame.AuthSteamReqFormatter();
                case 63: return new Haegin.Formatters.HaeginGame.AuthSteamResFormatter();
                case 64: return new Haegin.Formatters.HaeginGame.SteamFinalizeTransactionReqFormatter();
                case 65: return new Haegin.Formatters.HaeginGame.SteamFinalizeTransactionResFormatter();
                case 66: return new Haegin.Formatters.HaeginGame.SteamInitTransactionReqFormatter();
                case 67: return new Haegin.Formatters.HaeginGame.SteamInitTransactionResFormatter();
                case 68: return new Haegin.Formatters.HaeginGame.ShopProductListReqFormatter();
                case 69: return new Haegin.Formatters.HaeginGame.ShopProductInfoFormatter();
                case 70: return new Haegin.Formatters.HaeginGame.ShopProductListResFormatter();
                case 71: return new Haegin.Formatters.HaeginGame.MaintenanceCheckReqFormatter();
                case 72: return new Haegin.Formatters.HaeginGame.MaintenanceCheckResFormatter();
                case 73: return new Haegin.Formatters.HaeginGame.NoticeReqFormatter();
                case 74: return new Haegin.Formatters.HaeginGame.NoticeResFormatter();
                case 75: return new Haegin.Formatters.HaeginGame.KeyCountReqFormatter();
                case 76: return new Haegin.Formatters.HaeginGame.KeyCountResFormatter();
                case 77: return new Haegin.Formatters.HaeginGame.CouponReqFormatter();
                case 78: return new Haegin.Formatters.HaeginGame.CouponResFormatter();
                case 79: return new Haegin.Formatters.HaeginGame.MaintenanceCheckV2ReqFormatter();
                case 80: return new Haegin.Formatters.HaeginGame.MaintenanceCheckV2ResFormatter();
                case 81: return new Haegin.Formatters.HaeginGame.OneStorePurchasedProductFormatter();
                case 82: return new Haegin.Formatters.HaeginGame.ConsumeOneStoreReceiptReqFormatter();
                case 83: return new Haegin.Formatters.HaeginGame.ConsumeOneStoreReceiptResFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Haegin.Formatters.HaeginGame
{
    using System;
    using MessagePack;

    public sealed class ProtocolIdFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ProtocolId>
    {
        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ProtocolId value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::HaeginGame.ProtocolId Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::HaeginGame.ProtocolId)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class TransactionStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.TransactionState>
    {
        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.TransactionState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::HaeginGame.TransactionState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::HaeginGame.TransactionState)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class ConsumptionStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumptionState>
    {
        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumptionState value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::HaeginGame.ConsumptionState Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::HaeginGame.ConsumptionState)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Haegin.Formatters.Haegin
{
    using System;
    using MessagePack;

    public sealed class BannerTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.BannerType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.BannerType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.BannerType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.BannerType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class ResultFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.Result>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.Result value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.Result Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.Result)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class MarketTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.MarketType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.MarketType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.MarketType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.MarketType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class OsTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.OsType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.OsType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.OsType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.OsType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class AccountTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.AccountType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.AccountType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteSByte(ref bytes, offset, (SByte)value);
        }
        
        public global::Haegin.AccountType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.AccountType)MessagePackBinary.ReadSByte(bytes, offset, out readSize);
        }
    }

    public sealed class BlockTypeFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.BlockType>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.BlockType value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.BlockType Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.BlockType)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class LinkOptionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.LinkOption>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.LinkOption value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.LinkOption Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.LinkOption)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }

    public sealed class LanguageFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.Language>
    {
        public int Serialize(ref byte[] bytes, int offset, global::Haegin.Language value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteInt32(ref bytes, offset, (Int32)value);
        }
        
        public global::Haegin.Language Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            return (global::Haegin.Language)MessagePackBinary.ReadInt32(bytes, offset, out readSize);
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Haegin.Formatters.HaeginGame
{
    using System;
    using System.Collections.Generic;
    using MessagePack;

    public sealed class ProtocolFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.Protocol>
    {
        readonly Dictionary<RuntimeTypeHandle, KeyValuePair<int, int>> typeToKeyAndJumpMap;
        readonly Dictionary<int, int> keyToJumpMap;

        public ProtocolFormatter()
        {
            this.typeToKeyAndJumpMap = new Dictionary<RuntimeTypeHandle, KeyValuePair<int, int>>(24, global::MessagePack.Internal.RuntimeTypeHandleEqualityComparer.Default)
            {
                { typeof(global::HaeginGame.MaintenanceCheckReq).TypeHandle, new KeyValuePair<int, int>(1, 0) },
                { typeof(global::HaeginGame.MaintenanceCheckV2Req).TypeHandle, new KeyValuePair<int, int>(2, 1) },
                { typeof(global::HaeginGame.HandshakeReq).TypeHandle, new KeyValuePair<int, int>(101, 2) },
                { typeof(global::HaeginGame.AuthReq).TypeHandle, new KeyValuePair<int, int>(102, 3) },
                { typeof(global::HaeginGame.AuthAppleReq).TypeHandle, new KeyValuePair<int, int>(103, 4) },
                { typeof(global::HaeginGame.AuthGoogleReq).TypeHandle, new KeyValuePair<int, int>(104, 5) },
                { typeof(global::HaeginGame.AuthFacebookReq).TypeHandle, new KeyValuePair<int, int>(105, 6) },
                { typeof(global::HaeginGame.AuthSteamReq).TypeHandle, new KeyValuePair<int, int>(106, 7) },
                { typeof(global::HaeginGame.AliveReq).TypeHandle, new KeyValuePair<int, int>(201, 8) },
                { typeof(global::HaeginGame.TermsListReq).TypeHandle, new KeyValuePair<int, int>(202, 9) },
                { typeof(global::HaeginGame.TermsConfirmReq).TypeHandle, new KeyValuePair<int, int>(203, 10) },
                { typeof(global::HaeginGame.ApnsRegisterReq).TypeHandle, new KeyValuePair<int, int>(301, 11) },
                { typeof(global::HaeginGame.FcmRegisterReq).TypeHandle, new KeyValuePair<int, int>(302, 12) },
                { typeof(global::HaeginGame.FcmUnregisterReq).TypeHandle, new KeyValuePair<int, int>(303, 13) },
                { typeof(global::HaeginGame.ShopProductListReq).TypeHandle, new KeyValuePair<int, int>(401, 14) },
                { typeof(global::HaeginGame.ConsumeAppleReceiptReq).TypeHandle, new KeyValuePair<int, int>(402, 15) },
                { typeof(global::HaeginGame.ConsumeGoogleReceiptReq).TypeHandle, new KeyValuePair<int, int>(403, 16) },
                { typeof(global::HaeginGame.SteamInitTransactionReq).TypeHandle, new KeyValuePair<int, int>(404, 17) },
                { typeof(global::HaeginGame.SteamFinalizeTransactionReq).TypeHandle, new KeyValuePair<int, int>(405, 18) },
                { typeof(global::HaeginGame.ConsumeOneStoreReceiptReq).TypeHandle, new KeyValuePair<int, int>(406, 19) },
                { typeof(global::HaeginGame.EventListReq).TypeHandle, new KeyValuePair<int, int>(501, 20) },
                { typeof(global::HaeginGame.NoticeReq).TypeHandle, new KeyValuePair<int, int>(502, 21) },
                { typeof(global::HaeginGame.KeyCountReq).TypeHandle, new KeyValuePair<int, int>(601, 22) },
                { typeof(global::HaeginGame.CouponReq).TypeHandle, new KeyValuePair<int, int>(701, 23) },
            };
            this.keyToJumpMap = new Dictionary<int, int>(24)
            {
                { 1, 0 },
                { 2, 1 },
                { 101, 2 },
                { 102, 3 },
                { 103, 4 },
                { 104, 5 },
                { 105, 6 },
                { 106, 7 },
                { 201, 8 },
                { 202, 9 },
                { 203, 10 },
                { 301, 11 },
                { 302, 12 },
                { 303, 13 },
                { 401, 14 },
                { 402, 15 },
                { 403, 16 },
                { 404, 17 },
                { 405, 18 },
                { 406, 19 },
                { 501, 20 },
                { 502, 21 },
                { 601, 22 },
                { 701, 23 },
            };
        }

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.Protocol value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            KeyValuePair<int, int> keyValuePair;
            if (value != null && this.typeToKeyAndJumpMap.TryGetValue(value.GetType().TypeHandle, out keyValuePair))
            {
                var startOffset = offset;
                offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
                offset += MessagePackBinary.WriteInt32(ref bytes, offset, keyValuePair.Key);
                switch (keyValuePair.Value)
                {
                    case 0:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckReq>().Serialize(ref bytes, offset, (global::HaeginGame.MaintenanceCheckReq)value, formatterResolver);
                        break;
                    case 1:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckV2Req>().Serialize(ref bytes, offset, (global::HaeginGame.MaintenanceCheckV2Req)value, formatterResolver);
                        break;
                    case 2:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.HandshakeReq>().Serialize(ref bytes, offset, (global::HaeginGame.HandshakeReq)value, formatterResolver);
                        break;
                    case 3:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthReq>().Serialize(ref bytes, offset, (global::HaeginGame.AuthReq)value, formatterResolver);
                        break;
                    case 4:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthAppleReq>().Serialize(ref bytes, offset, (global::HaeginGame.AuthAppleReq)value, formatterResolver);
                        break;
                    case 5:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthGoogleReq>().Serialize(ref bytes, offset, (global::HaeginGame.AuthGoogleReq)value, formatterResolver);
                        break;
                    case 6:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthFacebookReq>().Serialize(ref bytes, offset, (global::HaeginGame.AuthFacebookReq)value, formatterResolver);
                        break;
                    case 7:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthSteamReq>().Serialize(ref bytes, offset, (global::HaeginGame.AuthSteamReq)value, formatterResolver);
                        break;
                    case 8:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AliveReq>().Serialize(ref bytes, offset, (global::HaeginGame.AliveReq)value, formatterResolver);
                        break;
                    case 9:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsListReq>().Serialize(ref bytes, offset, (global::HaeginGame.TermsListReq)value, formatterResolver);
                        break;
                    case 10:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsConfirmReq>().Serialize(ref bytes, offset, (global::HaeginGame.TermsConfirmReq)value, formatterResolver);
                        break;
                    case 11:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ApnsRegisterReq>().Serialize(ref bytes, offset, (global::HaeginGame.ApnsRegisterReq)value, formatterResolver);
                        break;
                    case 12:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmRegisterReq>().Serialize(ref bytes, offset, (global::HaeginGame.FcmRegisterReq)value, formatterResolver);
                        break;
                    case 13:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmUnregisterReq>().Serialize(ref bytes, offset, (global::HaeginGame.FcmUnregisterReq)value, formatterResolver);
                        break;
                    case 14:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ShopProductListReq>().Serialize(ref bytes, offset, (global::HaeginGame.ShopProductListReq)value, formatterResolver);
                        break;
                    case 15:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeAppleReceiptReq>().Serialize(ref bytes, offset, (global::HaeginGame.ConsumeAppleReceiptReq)value, formatterResolver);
                        break;
                    case 16:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeGoogleReceiptReq>().Serialize(ref bytes, offset, (global::HaeginGame.ConsumeGoogleReceiptReq)value, formatterResolver);
                        break;
                    case 17:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamInitTransactionReq>().Serialize(ref bytes, offset, (global::HaeginGame.SteamInitTransactionReq)value, formatterResolver);
                        break;
                    case 18:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamFinalizeTransactionReq>().Serialize(ref bytes, offset, (global::HaeginGame.SteamFinalizeTransactionReq)value, formatterResolver);
                        break;
                    case 19:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeOneStoreReceiptReq>().Serialize(ref bytes, offset, (global::HaeginGame.ConsumeOneStoreReceiptReq)value, formatterResolver);
                        break;
                    case 20:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.EventListReq>().Serialize(ref bytes, offset, (global::HaeginGame.EventListReq)value, formatterResolver);
                        break;
                    case 21:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.NoticeReq>().Serialize(ref bytes, offset, (global::HaeginGame.NoticeReq)value, formatterResolver);
                        break;
                    case 22:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.KeyCountReq>().Serialize(ref bytes, offset, (global::HaeginGame.KeyCountReq)value, formatterResolver);
                        break;
                    case 23:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.CouponReq>().Serialize(ref bytes, offset, (global::HaeginGame.CouponReq)value, formatterResolver);
                        break;
                    default:
                        break;
                }

                return offset - startOffset;
            }

            return MessagePackBinary.WriteNil(ref bytes, offset);
        }
        
        public global::HaeginGame.Protocol Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            
            if (MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize) != 2)
            {
                throw new InvalidOperationException("Invalid Union data was detected. Type:global::HaeginGame.Protocol");
            }
            offset += readSize;

            var key = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
            offset += readSize;

            if (!this.keyToJumpMap.TryGetValue(key, out key))
            {
                key = -1;
            }

            global::HaeginGame.Protocol result = null;
            switch (key)
            {
                case 0:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 1:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckV2Req>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 2:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.HandshakeReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 3:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 4:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthAppleReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 5:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthGoogleReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 6:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthFacebookReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 7:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthSteamReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 8:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AliveReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 9:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsListReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 10:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsConfirmReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 11:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ApnsRegisterReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 12:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmRegisterReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 13:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmUnregisterReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 14:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ShopProductListReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 15:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeAppleReceiptReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 16:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeGoogleReceiptReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 17:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamInitTransactionReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 18:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamFinalizeTransactionReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 19:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeOneStoreReceiptReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 20:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.EventListReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 21:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.NoticeReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 22:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.KeyCountReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 23:
                    result = (global::HaeginGame.Protocol)formatterResolver.GetFormatterWithVerify<global::HaeginGame.CouponReq>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                default:
                    offset += MessagePackBinary.ReadNextBlock(bytes, offset);
                    break;
            }
            
            readSize = offset - startOffset;
            
            return result;
        }
    }

    public sealed class ProtocolResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ProtocolRes>
    {
        readonly Dictionary<RuntimeTypeHandle, KeyValuePair<int, int>> typeToKeyAndJumpMap;
        readonly Dictionary<int, int> keyToJumpMap;

        public ProtocolResFormatter()
        {
            this.typeToKeyAndJumpMap = new Dictionary<RuntimeTypeHandle, KeyValuePair<int, int>>(25, global::MessagePack.Internal.RuntimeTypeHandleEqualityComparer.Default)
            {
                { typeof(global::HaeginGame.MaintenanceCheckRes).TypeHandle, new KeyValuePair<int, int>(1, 0) },
                { typeof(global::HaeginGame.MaintenanceCheckV2Res).TypeHandle, new KeyValuePair<int, int>(2, 1) },
                { typeof(global::HaeginGame.ErrorRes).TypeHandle, new KeyValuePair<int, int>(99, 2) },
                { typeof(global::HaeginGame.HandshakeRes).TypeHandle, new KeyValuePair<int, int>(101, 3) },
                { typeof(global::HaeginGame.AuthRes).TypeHandle, new KeyValuePair<int, int>(102, 4) },
                { typeof(global::HaeginGame.AuthAppleRes).TypeHandle, new KeyValuePair<int, int>(103, 5) },
                { typeof(global::HaeginGame.AuthGoogleRes).TypeHandle, new KeyValuePair<int, int>(104, 6) },
                { typeof(global::HaeginGame.AuthFacebookRes).TypeHandle, new KeyValuePair<int, int>(105, 7) },
                { typeof(global::HaeginGame.AuthSteamRes).TypeHandle, new KeyValuePair<int, int>(106, 8) },
                { typeof(global::HaeginGame.AliveRes).TypeHandle, new KeyValuePair<int, int>(201, 9) },
                { typeof(global::HaeginGame.TermsListRes).TypeHandle, new KeyValuePair<int, int>(202, 10) },
                { typeof(global::HaeginGame.TermsConfirmRes).TypeHandle, new KeyValuePair<int, int>(203, 11) },
                { typeof(global::HaeginGame.ApnsRegisterRes).TypeHandle, new KeyValuePair<int, int>(301, 12) },
                { typeof(global::HaeginGame.FcmRegisterRes).TypeHandle, new KeyValuePair<int, int>(302, 13) },
                { typeof(global::HaeginGame.FcmUnregisterRes).TypeHandle, new KeyValuePair<int, int>(303, 14) },
                { typeof(global::HaeginGame.ShopProductListRes).TypeHandle, new KeyValuePair<int, int>(401, 15) },
                { typeof(global::HaeginGame.ConsumeAppleReceiptRes).TypeHandle, new KeyValuePair<int, int>(402, 16) },
                { typeof(global::HaeginGame.ConsumeGoogleReceiptRes).TypeHandle, new KeyValuePair<int, int>(403, 17) },
                { typeof(global::HaeginGame.SteamInitTransactionRes).TypeHandle, new KeyValuePair<int, int>(404, 18) },
                { typeof(global::HaeginGame.SteamFinalizeTransactionRes).TypeHandle, new KeyValuePair<int, int>(405, 19) },
                { typeof(global::HaeginGame.ConsumeOneStoreReceiptRes).TypeHandle, new KeyValuePair<int, int>(406, 20) },
                { typeof(global::HaeginGame.EventListRes).TypeHandle, new KeyValuePair<int, int>(501, 21) },
                { typeof(global::HaeginGame.NoticeRes).TypeHandle, new KeyValuePair<int, int>(502, 22) },
                { typeof(global::HaeginGame.KeyCountRes).TypeHandle, new KeyValuePair<int, int>(601, 23) },
                { typeof(global::HaeginGame.CouponRes).TypeHandle, new KeyValuePair<int, int>(701, 24) },
            };
            this.keyToJumpMap = new Dictionary<int, int>(25)
            {
                { 1, 0 },
                { 2, 1 },
                { 99, 2 },
                { 101, 3 },
                { 102, 4 },
                { 103, 5 },
                { 104, 6 },
                { 105, 7 },
                { 106, 8 },
                { 201, 9 },
                { 202, 10 },
                { 203, 11 },
                { 301, 12 },
                { 302, 13 },
                { 303, 14 },
                { 401, 15 },
                { 402, 16 },
                { 403, 17 },
                { 404, 18 },
                { 405, 19 },
                { 406, 20 },
                { 501, 21 },
                { 502, 22 },
                { 601, 23 },
                { 701, 24 },
            };
        }

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ProtocolRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            KeyValuePair<int, int> keyValuePair;
            if (value != null && this.typeToKeyAndJumpMap.TryGetValue(value.GetType().TypeHandle, out keyValuePair))
            {
                var startOffset = offset;
                offset += MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
                offset += MessagePackBinary.WriteInt32(ref bytes, offset, keyValuePair.Key);
                switch (keyValuePair.Value)
                {
                    case 0:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckRes>().Serialize(ref bytes, offset, (global::HaeginGame.MaintenanceCheckRes)value, formatterResolver);
                        break;
                    case 1:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckV2Res>().Serialize(ref bytes, offset, (global::HaeginGame.MaintenanceCheckV2Res)value, formatterResolver);
                        break;
                    case 2:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ErrorRes>().Serialize(ref bytes, offset, (global::HaeginGame.ErrorRes)value, formatterResolver);
                        break;
                    case 3:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.HandshakeRes>().Serialize(ref bytes, offset, (global::HaeginGame.HandshakeRes)value, formatterResolver);
                        break;
                    case 4:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthRes>().Serialize(ref bytes, offset, (global::HaeginGame.AuthRes)value, formatterResolver);
                        break;
                    case 5:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthAppleRes>().Serialize(ref bytes, offset, (global::HaeginGame.AuthAppleRes)value, formatterResolver);
                        break;
                    case 6:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthGoogleRes>().Serialize(ref bytes, offset, (global::HaeginGame.AuthGoogleRes)value, formatterResolver);
                        break;
                    case 7:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthFacebookRes>().Serialize(ref bytes, offset, (global::HaeginGame.AuthFacebookRes)value, formatterResolver);
                        break;
                    case 8:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthSteamRes>().Serialize(ref bytes, offset, (global::HaeginGame.AuthSteamRes)value, formatterResolver);
                        break;
                    case 9:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.AliveRes>().Serialize(ref bytes, offset, (global::HaeginGame.AliveRes)value, formatterResolver);
                        break;
                    case 10:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsListRes>().Serialize(ref bytes, offset, (global::HaeginGame.TermsListRes)value, formatterResolver);
                        break;
                    case 11:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsConfirmRes>().Serialize(ref bytes, offset, (global::HaeginGame.TermsConfirmRes)value, formatterResolver);
                        break;
                    case 12:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ApnsRegisterRes>().Serialize(ref bytes, offset, (global::HaeginGame.ApnsRegisterRes)value, formatterResolver);
                        break;
                    case 13:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmRegisterRes>().Serialize(ref bytes, offset, (global::HaeginGame.FcmRegisterRes)value, formatterResolver);
                        break;
                    case 14:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmUnregisterRes>().Serialize(ref bytes, offset, (global::HaeginGame.FcmUnregisterRes)value, formatterResolver);
                        break;
                    case 15:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ShopProductListRes>().Serialize(ref bytes, offset, (global::HaeginGame.ShopProductListRes)value, formatterResolver);
                        break;
                    case 16:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeAppleReceiptRes>().Serialize(ref bytes, offset, (global::HaeginGame.ConsumeAppleReceiptRes)value, formatterResolver);
                        break;
                    case 17:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeGoogleReceiptRes>().Serialize(ref bytes, offset, (global::HaeginGame.ConsumeGoogleReceiptRes)value, formatterResolver);
                        break;
                    case 18:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamInitTransactionRes>().Serialize(ref bytes, offset, (global::HaeginGame.SteamInitTransactionRes)value, formatterResolver);
                        break;
                    case 19:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamFinalizeTransactionRes>().Serialize(ref bytes, offset, (global::HaeginGame.SteamFinalizeTransactionRes)value, formatterResolver);
                        break;
                    case 20:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeOneStoreReceiptRes>().Serialize(ref bytes, offset, (global::HaeginGame.ConsumeOneStoreReceiptRes)value, formatterResolver);
                        break;
                    case 21:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.EventListRes>().Serialize(ref bytes, offset, (global::HaeginGame.EventListRes)value, formatterResolver);
                        break;
                    case 22:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.NoticeRes>().Serialize(ref bytes, offset, (global::HaeginGame.NoticeRes)value, formatterResolver);
                        break;
                    case 23:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.KeyCountRes>().Serialize(ref bytes, offset, (global::HaeginGame.KeyCountRes)value, formatterResolver);
                        break;
                    case 24:
                        offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.CouponRes>().Serialize(ref bytes, offset, (global::HaeginGame.CouponRes)value, formatterResolver);
                        break;
                    default:
                        break;
                }

                return offset - startOffset;
            }

            return MessagePackBinary.WriteNil(ref bytes, offset);
        }
        
        public global::HaeginGame.ProtocolRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            
            if (MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize) != 2)
            {
                throw new InvalidOperationException("Invalid Union data was detected. Type:global::HaeginGame.ProtocolRes");
            }
            offset += readSize;

            var key = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
            offset += readSize;

            if (!this.keyToJumpMap.TryGetValue(key, out key))
            {
                key = -1;
            }

            global::HaeginGame.ProtocolRes result = null;
            switch (key)
            {
                case 0:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 1:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.MaintenanceCheckV2Res>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 2:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ErrorRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 3:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.HandshakeRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 4:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 5:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthAppleRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 6:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthGoogleRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 7:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthFacebookRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 8:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AuthSteamRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 9:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.AliveRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 10:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsListRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 11:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.TermsConfirmRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 12:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ApnsRegisterRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 13:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmRegisterRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 14:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.FcmUnregisterRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 15:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ShopProductListRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 16:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeAppleReceiptRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 17:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeGoogleReceiptRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 18:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamInitTransactionRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 19:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.SteamFinalizeTransactionRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 20:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumeOneStoreReceiptRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 21:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.EventListRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 22:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.NoticeRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 23:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.KeyCountRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                case 24:
                    result = (global::HaeginGame.ProtocolRes)formatterResolver.GetFormatterWithVerify<global::HaeginGame.CouponRes>().Deserialize(bytes, offset, formatterResolver, out readSize);
                    offset += readSize;
                    break;
                default:
                    offset += MessagePackBinary.ReadNextBlock(bytes, offset);
                    break;
            }
            
            readSize = offset - startOffset;
            
            return result;
        }
    }


}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Haegin.Formatters.Haegin
{
    using System;
    using MessagePack;


    public sealed class PlayerFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.Player>
    {

        public int Serialize(ref byte[] bytes, int offset, global::Haegin.Player value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.PlayerId);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Gold);
            return offset - startOffset;
        }

        public global::Haegin.Player Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PlayerId__ = default(long);
            var __Gold__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Gold__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Haegin.Player();
            ____result.PlayerId = __PlayerId__;
            ____result.Gold = __Gold__;
            return ____result;
        }
    }


    public sealed class PurchasedInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Haegin.PurchasedInfo>
    {

        public int Serialize(ref byte[] bytes, int offset, global::Haegin.PurchasedInfo value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.TransactionId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ProductId, formatterResolver);
            return offset - startOffset;
        }

        public global::Haegin.PurchasedInfo Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __TransactionId__ = default(string);
            var __ProductId__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __TransactionId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __ProductId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::Haegin.PurchasedInfo();
            ____result.TransactionId = __TransactionId__;
            ____result.ProductId = __ProductId__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

namespace Haegin.Formatters.HaeginGame
{
    using System;
    using MessagePack;


    public sealed class AliveReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AliveReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AliveReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AliveReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AliveReq();
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class AliveResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AliveRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AliveRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 8);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Seq);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.BannerType>().Serialize(ref bytes, offset, value.Type, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Title, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Sub, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Message, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.RemainedTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AliveRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Seq__ = default(int);
            var __Type__ = default(global::Haegin.BannerType);
            var __Title__ = default(string);
            var __Sub__ = default(string);
            var __Message__ = default(string);
            var __RemainedTime__ = default(global::System.TimeSpan);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Seq__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Type__ = formatterResolver.GetFormatterWithVerify<global::Haegin.BannerType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Title__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Sub__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __Message__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __RemainedTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AliveRes();
            ____result.Seq = __Seq__;
            ____result.Type = __Type__;
            ____result.Title = __Title__;
            ____result.Sub = __Sub__;
            ____result.Message = __Message__;
            ____result.RemainedTime = __RemainedTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class ApnsRegisterReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ApnsRegisterReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ApnsRegisterReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceToken, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ApnsRegisterReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __DeviceToken__ = default(string);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __DeviceToken__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ApnsRegisterReq();
            ____result.DeviceToken = __DeviceToken__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class ApnsRegisterResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ApnsRegisterRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ApnsRegisterRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ApnsRegisterRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ApnsRegisterRes();
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class AuthReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 9);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Duid, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Nickname, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Serialize(ref bytes, offset, value.Market, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Serialize(ref bytes, offset, value.Os, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GpKey, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Duid__ = default(string);
            var __Nickname__ = default(string);
            var __Market__ = default(global::Haegin.MarketType);
            var __Os__ = default(global::Haegin.OsType);
            var __ClientVersion__ = default(string);
            var __DeviceInfo__ = default(string);
            var __GpKey__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Duid__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Nickname__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Market__ = formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Os__ = formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __DeviceInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __GpKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthReq();
            ____result.AccountPass = __AccountPass__;
            ____result.Duid = __Duid__;
            ____result.Nickname = __Nickname__;
            ____result.Market = __Market__;
            ____result.Os = __Os__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.DeviceInfo = __DeviceInfo__;
            ____result.GpKey = __GpKey__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class AuthResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 9);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Suid);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Hash);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Serialize(ref bytes, offset, value.AccountType, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.BlockedSuid);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Serialize(ref bytes, offset, value.BlockType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.BlockRemainTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Suid__ = default(long);
            var __Hash__ = default(uint);
            var __AccountType__ = default(global::Haegin.AccountType);
            var __BlockedSuid__ = default(long);
            var __BlockType__ = default(global::Haegin.BlockType);
            var __BlockRemainTime__ = default(global::System.TimeSpan);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Suid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Hash__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __AccountType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __BlockedSuid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 7:
                        __BlockType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __BlockRemainTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthRes();
            ____result.AccountPass = __AccountPass__;
            ____result.Suid = __Suid__;
            ____result.Hash = __Hash__;
            ____result.AccountType = __AccountType__;
            ____result.BlockedSuid = __BlockedSuid__;
            ____result.BlockType = __BlockType__;
            ____result.BlockRemainTime = __BlockRemainTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class GameCenterSignatureFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.GameCenterSignature>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.GameCenterSignature value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PublicKeyUrl, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Signature, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Timestamp);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Salt, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.GameCenterSignature Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PublicKeyUrl__ = default(string);
            var __Signature__ = default(string);
            var __Timestamp__ = default(long);
            var __Salt__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __PublicKeyUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Signature__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Timestamp__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Salt__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.GameCenterSignature();
            ____result.PublicKeyUrl = __PublicKeyUrl__;
            ____result.Signature = __Signature__;
            ____result.Timestamp = __Timestamp__;
            ____result.Salt = __Salt__;
            return ____result;
        }
    }


    public sealed class AuthAppleReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthAppleReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthAppleReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 12);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Serialize(ref bytes, offset, value.Link, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PlayerId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PlayerAlias, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.GameCenterSignature>().Serialize(ref bytes, offset, value.Signature, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Serialize(ref bytes, offset, value.Market, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Serialize(ref bytes, offset, value.Os, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GpKey, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthAppleReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Link__ = default(global::Haegin.LinkOption);
            var __PlayerId__ = default(string);
            var __PlayerAlias__ = default(string);
            var __Signature__ = default(global::HaeginGame.GameCenterSignature);
            var __Name__ = default(string);
            var __Market__ = default(global::Haegin.MarketType);
            var __Os__ = default(global::Haegin.OsType);
            var __ClientVersion__ = default(string);
            var __DeviceInfo__ = default(string);
            var __GpKey__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Link__ = formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __PlayerId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __PlayerAlias__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Signature__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.GameCenterSignature>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __Market__ = formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __Os__ = formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __DeviceInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __GpKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthAppleReq();
            ____result.AccountPass = __AccountPass__;
            ____result.Link = __Link__;
            ____result.PlayerId = __PlayerId__;
            ____result.PlayerAlias = __PlayerAlias__;
            ____result.Signature = __Signature__;
            ____result.Name = __Name__;
            ____result.Market = __Market__;
            ____result.Os = __Os__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.DeviceInfo = __DeviceInfo__;
            ____result.GpKey = __GpKey__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class AuthAppleResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthAppleRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthAppleRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 12);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Suid);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Hash);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Serialize(ref bytes, offset, value.LocalAccountType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.ExtraData, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.BlockedSuid);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Serialize(ref bytes, offset, value.BlockType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.BlockRemainTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthAppleRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Suid__ = default(long);
            var __Hash__ = default(uint);
            var __LocalAccountType__ = default(global::Haegin.AccountType);
            var __LocalAccountId__ = default(string);
            var __LocalAccountName__ = default(string);
            var __ExtraData__ = default(byte[]);
            var __BlockedSuid__ = default(long);
            var __BlockType__ = default(global::Haegin.BlockType);
            var __BlockRemainTime__ = default(global::System.TimeSpan);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Suid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Hash__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LocalAccountType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __LocalAccountId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __LocalAccountName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __ExtraData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __BlockedSuid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 10:
                        __BlockType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __BlockRemainTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthAppleRes();
            ____result.AccountPass = __AccountPass__;
            ____result.Suid = __Suid__;
            ____result.Hash = __Hash__;
            ____result.LocalAccountType = __LocalAccountType__;
            ____result.LocalAccountId = __LocalAccountId__;
            ____result.LocalAccountName = __LocalAccountName__;
            ____result.ExtraData = __ExtraData__;
            ____result.BlockedSuid = __BlockedSuid__;
            ____result.BlockType = __BlockType__;
            ____result.BlockRemainTime = __BlockRemainTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class AuthFacebookReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthFacebookReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthFacebookReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 11);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Serialize(ref bytes, offset, value.Link, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.UserId);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccessToken, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Serialize(ref bytes, offset, value.Market, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Serialize(ref bytes, offset, value.Os, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GpKey, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthFacebookReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Link__ = default(global::Haegin.LinkOption);
            var __UserId__ = default(long);
            var __AccessToken__ = default(string);
            var __Name__ = default(string);
            var __Market__ = default(global::Haegin.MarketType);
            var __Os__ = default(global::Haegin.OsType);
            var __ClientVersion__ = default(string);
            var __DeviceInfo__ = default(string);
            var __GpKey__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Link__ = formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __UserId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __AccessToken__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __Market__ = formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __Os__ = formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __DeviceInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __GpKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthFacebookReq();
            ____result.AccountPass = __AccountPass__;
            ____result.Link = __Link__;
            ____result.UserId = __UserId__;
            ____result.AccessToken = __AccessToken__;
            ____result.Name = __Name__;
            ____result.Market = __Market__;
            ____result.Os = __Os__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.DeviceInfo = __DeviceInfo__;
            ____result.GpKey = __GpKey__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class AuthFacebookResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthFacebookRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthFacebookRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 12);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Suid);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Hash);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Serialize(ref bytes, offset, value.LocalAccountType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.ExtraData, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.BlockedSuid);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Serialize(ref bytes, offset, value.BlockType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.BlockRemainTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthFacebookRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Suid__ = default(long);
            var __Hash__ = default(uint);
            var __LocalAccountType__ = default(global::Haegin.AccountType);
            var __LocalAccountId__ = default(string);
            var __LocalAccountName__ = default(string);
            var __ExtraData__ = default(byte[]);
            var __BlockedSuid__ = default(long);
            var __BlockType__ = default(global::Haegin.BlockType);
            var __BlockRemainTime__ = default(global::System.TimeSpan);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Suid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Hash__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LocalAccountType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __LocalAccountId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __LocalAccountName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __ExtraData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __BlockedSuid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 10:
                        __BlockType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __BlockRemainTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthFacebookRes();
            ____result.AccountPass = __AccountPass__;
            ____result.Suid = __Suid__;
            ____result.Hash = __Hash__;
            ____result.LocalAccountType = __LocalAccountType__;
            ____result.LocalAccountId = __LocalAccountId__;
            ____result.LocalAccountName = __LocalAccountName__;
            ____result.ExtraData = __ExtraData__;
            ____result.BlockedSuid = __BlockedSuid__;
            ____result.BlockType = __BlockType__;
            ____result.BlockRemainTime = __BlockRemainTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class AuthGoogleReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthGoogleReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthGoogleReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 10);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Serialize(ref bytes, offset, value.Link, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AuthCode, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Serialize(ref bytes, offset, value.Market, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Serialize(ref bytes, offset, value.Os, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GpKey, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthGoogleReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Link__ = default(global::Haegin.LinkOption);
            var __AuthCode__ = default(string);
            var __Name__ = default(string);
            var __Market__ = default(global::Haegin.MarketType);
            var __Os__ = default(global::Haegin.OsType);
            var __ClientVersion__ = default(string);
            var __DeviceInfo__ = default(string);
            var __GpKey__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Link__ = formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __AuthCode__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Market__ = formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __Os__ = formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __DeviceInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __GpKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthGoogleReq();
            ____result.AccountPass = __AccountPass__;
            ____result.Link = __Link__;
            ____result.AuthCode = __AuthCode__;
            ____result.Name = __Name__;
            ____result.Market = __Market__;
            ____result.Os = __Os__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.DeviceInfo = __DeviceInfo__;
            ____result.GpKey = __GpKey__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class AuthGoogleResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthGoogleRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthGoogleRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 12);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Suid);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Hash);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Serialize(ref bytes, offset, value.LocalAccountType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.ExtraData, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.BlockedSuid);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Serialize(ref bytes, offset, value.BlockType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.BlockRemainTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthGoogleRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Suid__ = default(long);
            var __Hash__ = default(uint);
            var __LocalAccountType__ = default(global::Haegin.AccountType);
            var __LocalAccountId__ = default(string);
            var __LocalAccountName__ = default(string);
            var __ExtraData__ = default(byte[]);
            var __BlockedSuid__ = default(long);
            var __BlockType__ = default(global::Haegin.BlockType);
            var __BlockRemainTime__ = default(global::System.TimeSpan);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Suid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Hash__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LocalAccountType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __LocalAccountId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __LocalAccountName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __ExtraData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __BlockedSuid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 10:
                        __BlockType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __BlockRemainTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthGoogleRes();
            ____result.AccountPass = __AccountPass__;
            ____result.Suid = __Suid__;
            ____result.Hash = __Hash__;
            ____result.LocalAccountType = __LocalAccountType__;
            ____result.LocalAccountId = __LocalAccountId__;
            ____result.LocalAccountName = __LocalAccountName__;
            ____result.ExtraData = __ExtraData__;
            ____result.BlockedSuid = __BlockedSuid__;
            ____result.BlockType = __BlockType__;
            ____result.BlockRemainTime = __BlockRemainTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class ConsumeAppleReceiptReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumeAppleReceiptReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumeAppleReceiptReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Receipt, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ConsumeAppleReceiptReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Receipt__ = default(string);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Receipt__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ConsumeAppleReceiptReq();
            ____result.Receipt = __Receipt__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class StoreKitTransactionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.StoreKitTransaction>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.StoreKitTransaction value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ProductId, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.TransactionId);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Quantity);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.TransactionState>().Serialize(ref bytes, offset, value.TransactionState, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.StoreKitTransaction Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProductId__ = default(string);
            var __TransactionId__ = default(long);
            var __Quantity__ = default(int);
            var __TransactionState__ = default(global::HaeginGame.TransactionState);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __ProductId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __TransactionId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Quantity__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __TransactionState__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.TransactionState>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.StoreKitTransaction();
            ____result.ProductId = __ProductId__;
            ____result.TransactionId = __TransactionId__;
            ____result.Quantity = __Quantity__;
            ____result.TransactionState = __TransactionState__;
            return ____result;
        }
    }


    public sealed class ConsumeAppleReceiptResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumeAppleReceiptRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumeAppleReceiptRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.StoreKitTransaction>>().Serialize(ref bytes, offset, value.Transactions, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.PurchasedData, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ConsumeAppleReceiptRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Transactions__ = default(global::System.Collections.Generic.List<global::HaeginGame.StoreKitTransaction>);
            var __PurchasedData__ = default(byte[]);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Transactions__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.StoreKitTransaction>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __PurchasedData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ConsumeAppleReceiptRes();
            ____result.Transactions = __Transactions__;
            ____result.PurchasedData = __PurchasedData__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class GooglePurchasedProductFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.GooglePurchasedProduct>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.GooglePurchasedProduct value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PackageName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.OrderId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ProductId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Token, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.GooglePurchasedProduct Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __PackageName__ = default(string);
            var __OrderId__ = default(string);
            var __ProductId__ = default(string);
            var __Token__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __PackageName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __OrderId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __ProductId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Token__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.GooglePurchasedProduct();
            ____result.PackageName = __PackageName__;
            ____result.OrderId = __OrderId__;
            ____result.ProductId = __ProductId__;
            ____result.Token = __Token__;
            return ____result;
        }
    }


    public sealed class ConsumeGoogleReceiptReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumeGoogleReceiptReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumeGoogleReceiptReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.GooglePurchasedProduct>>().Serialize(ref bytes, offset, value.Products, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ConsumeGoogleReceiptReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Products__ = default(global::System.Collections.Generic.List<global::HaeginGame.GooglePurchasedProduct>);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Products__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.GooglePurchasedProduct>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ConsumeGoogleReceiptReq();
            ____result.Products = __Products__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class GoogleConsumedProductFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.GoogleConsumedProduct>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.GoogleConsumedProduct value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.OrderId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ProductId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumptionState>().Serialize(ref bytes, offset, value.ConsumptionState, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.PurchaseState);
            return offset - startOffset;
        }

        public global::HaeginGame.GoogleConsumedProduct Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __OrderId__ = default(string);
            var __ProductId__ = default(string);
            var __ConsumptionState__ = default(global::HaeginGame.ConsumptionState);
            var __PurchaseState__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __OrderId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __ProductId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __ConsumptionState__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumptionState>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __PurchaseState__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.GoogleConsumedProduct();
            ____result.OrderId = __OrderId__;
            ____result.ProductId = __ProductId__;
            ____result.ConsumptionState = __ConsumptionState__;
            ____result.PurchaseState = __PurchaseState__;
            return ____result;
        }
    }


    public sealed class ConsumeGoogleReceiptResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumeGoogleReceiptRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumeGoogleReceiptRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.GoogleConsumedProduct>>().Serialize(ref bytes, offset, value.Products, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.PurchasedData, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ConsumeGoogleReceiptRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Products__ = default(global::System.Collections.Generic.List<global::HaeginGame.GoogleConsumedProduct>);
            var __PurchasedData__ = default(byte[]);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Products__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.GoogleConsumedProduct>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __PurchasedData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ConsumeGoogleReceiptRes();
            ____result.Products = __Products__;
            ____result.PurchasedData = __PurchasedData__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class ErrorResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ErrorRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ErrorRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ErrorRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ErrorRes();
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class EventListReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.EventListReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.EventListReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 1);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.EventListReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.EventListReq();
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class EventItemFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.EventItem>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.EventItem value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Image, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Uri, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.StartTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.EndTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.EventItem Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Image__ = default(string);
            var __Uri__ = default(string);
            var __StartTime__ = default(global::System.DateTime);
            var __EndTime__ = default(global::System.DateTime);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Image__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Uri__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __StartTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __EndTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.EventItem();
            ____result.Image = __Image__;
            ____result.Uri = __Uri__;
            ____result.StartTime = __StartTime__;
            ____result.EndTime = __EndTime__;
            return ____result;
        }
    }


    public sealed class EventListResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.EventListRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.EventListRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.EventItem>>().Serialize(ref bytes, offset, value.Events, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.EventListRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Events__ = default(global::System.Collections.Generic.List<global::HaeginGame.EventItem>);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Events__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.EventItem>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.EventListRes();
            ____result.Events = __Events__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class FcmRegisterReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.FcmRegisterReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.FcmRegisterReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.RegistrationId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.FcmRegisterReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __RegistrationId__ = default(string);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __RegistrationId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.FcmRegisterReq();
            ____result.RegistrationId = __RegistrationId__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class FcmRegisterResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.FcmRegisterRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.FcmRegisterRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.FcmRegisterRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.FcmRegisterRes();
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class FcmUnregisterReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.FcmUnregisterReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.FcmUnregisterReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.RegistrationId, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.FcmUnregisterReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __RegistrationId__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __RegistrationId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.FcmUnregisterReq();
            ____result.RegistrationId = __RegistrationId__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class FcmUnregisterResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.FcmUnregisterRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.FcmUnregisterRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.FcmUnregisterRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.FcmUnregisterRes();
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class HandshakeReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.HandshakeReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.HandshakeReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<ushort[]>().Serialize(ref bytes, offset, value.ProtocolVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<ushort[]>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Serialize(ref bytes, offset, value.MarketType, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.HandshakeReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolVersion__ = default(ushort[]);
            var __ClientVersion__ = default(ushort[]);
            var __Language__ = default(string);
            var __MarketType__ = default(global::Haegin.MarketType);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __ProtocolVersion__ = formatterResolver.GetFormatterWithVerify<ushort[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<ushort[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __MarketType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.HandshakeReq();
            ____result.ProtocolVersion = __ProtocolVersion__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.Language = __Language__;
            ____result.MarketType = __MarketType__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class HandshakeResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.HandshakeRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.HandshakeRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 11);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.ProtocolVersionOK);
            offset += formatterResolver.GetFormatterWithVerify<ushort[]>().Serialize(ref bytes, offset, value.ProtocolVersion, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.ClientVersionOK);
            offset += formatterResolver.GetFormatterWithVerify<ushort[]>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Key1);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Key2);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Key3);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Key4);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ContentsForUpdate, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.HandshakeRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolVersionOK__ = default(bool);
            var __ProtocolVersion__ = default(ushort[]);
            var __ClientVersionOK__ = default(bool);
            var __ClientVersion__ = default(ushort[]);
            var __Key1__ = default(uint);
            var __Key2__ = default(uint);
            var __Key3__ = default(uint);
            var __Key4__ = default(uint);
            var __ContentsForUpdate__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __ProtocolVersionOK__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 3:
                        __ProtocolVersion__ = formatterResolver.GetFormatterWithVerify<ushort[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __ClientVersionOK__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 5:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<ushort[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __Key1__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __Key2__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 8:
                        __Key3__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 9:
                        __Key4__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 10:
                        __ContentsForUpdate__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.HandshakeRes();
            ____result.ProtocolVersionOK = __ProtocolVersionOK__;
            ____result.ProtocolVersion = __ProtocolVersion__;
            ____result.ClientVersionOK = __ClientVersionOK__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.Key1 = __Key1__;
            ____result.Key2 = __Key2__;
            ____result.Key3 = __Key3__;
            ____result.Key4 = __Key4__;
            ____result.ContentsForUpdate = __ContentsForUpdate__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class ProtocolReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ProtocolReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ProtocolReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Suid);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Ticks);
            offset += MessagePackBinary.WriteByte(ref bytes, offset, value.Retry);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.Protocol>().Serialize(ref bytes, offset, value.Protocol, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ProtocolReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Suid__ = default(long);
            var __Ticks__ = default(long);
            var __Retry__ = default(byte);
            var __Protocol__ = default(global::HaeginGame.Protocol);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __Suid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Ticks__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Retry__ = MessagePackBinary.ReadByte(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Protocol__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.Protocol>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ProtocolReq();
            ____result.Suid = __Suid__;
            ____result.Ticks = __Ticks__;
            ____result.Retry = __Retry__;
            ____result.Protocol = __Protocol__;
            return ____result;
        }
    }


    public sealed class TermsKindVersionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.TermsKindVersion>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.TermsKindVersion value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Kind);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Version);
            return offset - startOffset;
        }

        public global::HaeginGame.TermsKindVersion Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Kind__ = default(int);
            var __Version__ = default(int);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Kind__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Version__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.TermsKindVersion();
            ____result.Kind = __Kind__;
            ____result.Version = __Version__;
            return ____result;
        }
    }


    public sealed class TermsConfirmReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.TermsConfirmReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.TermsConfirmReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.TermsKindVersion>>().Serialize(ref bytes, offset, value.Confirms, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.TermsConfirmReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Confirms__ = default(global::System.Collections.Generic.List<global::HaeginGame.TermsKindVersion>);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Confirms__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.TermsKindVersion>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.TermsConfirmReq();
            ____result.Confirms = __Confirms__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class TermsConfirmResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.TermsConfirmRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.TermsConfirmRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.TermsConfirmRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.TermsConfirmRes();
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class TermsListReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.TermsListReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.TermsListReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Language>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.TermsListReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Language__ = default(global::Haegin.Language);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Language__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Language>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.TermsListReq();
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class TermsFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.Terms>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.Terms value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 7);
            offset += global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Kind);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Version);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Title, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Content, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsRequired);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsConfirmed);
            return offset - startOffset;
        }

        public global::HaeginGame.Terms Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Kind__ = default(int);
            var __Version__ = default(int);
            var __Title__ = default(string);
            var __Content__ = default(string);
            var __IsRequired__ = default(bool);
            var __IsConfirmed__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Kind__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Version__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Title__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Content__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __IsRequired__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 6:
                        __IsConfirmed__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.Terms();
            ____result.Kind = __Kind__;
            ____result.Version = __Version__;
            ____result.Title = __Title__;
            ____result.Content = __Content__;
            ____result.IsRequired = __IsRequired__;
            ____result.IsConfirmed = __IsConfirmed__;
            return ____result;
        }
    }


    public sealed class TermsListResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.TermsListRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.TermsListRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.Terms>>().Serialize(ref bytes, offset, value.List, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.TermsListRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __List__ = default(global::System.Collections.Generic.List<global::HaeginGame.Terms>);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __List__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.Terms>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.TermsListRes();
            ____result.List = __List__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class LineItemFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.LineItem>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.LineItem value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 5);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ItemId);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.Quantity);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Amount);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Description, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Category, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.LineItem Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ItemId__ = default(int);
            var __Quantity__ = default(int);
            var __Amount__ = default(long);
            var __Description__ = default(string);
            var __Category__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ItemId__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 1:
                        __Quantity__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Amount__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 3:
                        __Description__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __Category__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.LineItem();
            ____result.ItemId = __ItemId__;
            ____result.Quantity = __Quantity__;
            ____result.Amount = __Amount__;
            ____result.Description = __Description__;
            ____result.Category = __Category__;
            return ____result;
        }
    }


    public sealed class AuthSteamReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthSteamReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthSteamReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 11);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Serialize(ref bytes, offset, value.Link, formatterResolver);
            offset += MessagePackBinary.WriteUInt64(ref bytes, offset, value.SteamId);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.SessionTicket, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Name, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Serialize(ref bytes, offset, value.Market, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Serialize(ref bytes, offset, value.Os, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ClientVersion, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeviceInfo, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GpKey, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthSteamReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Link__ = default(global::Haegin.LinkOption);
            var __SteamId__ = default(ulong);
            var __SessionTicket__ = default(byte[]);
            var __Name__ = default(string);
            var __Market__ = default(global::Haegin.MarketType);
            var __Os__ = default(global::Haegin.OsType);
            var __ClientVersion__ = default(string);
            var __DeviceInfo__ = default(string);
            var __GpKey__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Link__ = formatterResolver.GetFormatterWithVerify<global::Haegin.LinkOption>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __SteamId__ = MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __SessionTicket__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __Name__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __Market__ = formatterResolver.GetFormatterWithVerify<global::Haegin.MarketType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __Os__ = formatterResolver.GetFormatterWithVerify<global::Haegin.OsType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __ClientVersion__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __DeviceInfo__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 10:
                        __GpKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthSteamReq();
            ____result.AccountPass = __AccountPass__;
            ____result.Link = __Link__;
            ____result.SteamId = __SteamId__;
            ____result.SessionTicket = __SessionTicket__;
            ____result.Name = __Name__;
            ____result.Market = __Market__;
            ____result.Os = __Os__;
            ____result.ClientVersion = __ClientVersion__;
            ____result.DeviceInfo = __DeviceInfo__;
            ____result.GpKey = __GpKey__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class AuthSteamResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.AuthSteamRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.AuthSteamRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 12);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.AccountPass, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.Suid);
            offset += MessagePackBinary.WriteUInt32(ref bytes, offset, value.Hash);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Serialize(ref bytes, offset, value.LocalAccountType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.LocalAccountName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.ExtraData, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.BlockedSuid);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Serialize(ref bytes, offset, value.BlockType, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Serialize(ref bytes, offset, value.BlockRemainTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.AuthSteamRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __AccountPass__ = default(string);
            var __Suid__ = default(long);
            var __Hash__ = default(uint);
            var __LocalAccountType__ = default(global::Haegin.AccountType);
            var __LocalAccountId__ = default(string);
            var __LocalAccountName__ = default(string);
            var __ExtraData__ = default(byte[]);
            var __BlockedSuid__ = default(long);
            var __BlockType__ = default(global::Haegin.BlockType);
            var __BlockRemainTime__ = default(global::System.TimeSpan);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __AccountPass__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Suid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __Hash__ = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
                        break;
                    case 5:
                        __LocalAccountType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.AccountType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __LocalAccountId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __LocalAccountName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __ExtraData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 9:
                        __BlockedSuid__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 10:
                        __BlockType__ = formatterResolver.GetFormatterWithVerify<global::Haegin.BlockType>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 11:
                        __BlockRemainTime__ = formatterResolver.GetFormatterWithVerify<global::System.TimeSpan>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.AuthSteamRes();
            ____result.AccountPass = __AccountPass__;
            ____result.Suid = __Suid__;
            ____result.Hash = __Hash__;
            ____result.LocalAccountType = __LocalAccountType__;
            ____result.LocalAccountId = __LocalAccountId__;
            ____result.LocalAccountName = __LocalAccountName__;
            ____result.ExtraData = __ExtraData__;
            ____result.BlockedSuid = __BlockedSuid__;
            ____result.BlockType = __BlockType__;
            ____result.BlockRemainTime = __BlockRemainTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class SteamFinalizeTransactionReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.SteamFinalizeTransactionReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.SteamFinalizeTransactionReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.OrderId);
            return offset - startOffset;
        }

        public global::HaeginGame.SteamFinalizeTransactionReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __OrderId__ = default(long);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __OrderId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.SteamFinalizeTransactionReq();
            ____result.OrderId = __OrderId__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class SteamFinalizeTransactionResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.SteamFinalizeTransactionRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.SteamFinalizeTransactionRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 6);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsSucceeded);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ErrorCode);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ErrorDesc, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.PurchasedData, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.SteamFinalizeTransactionRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __IsSucceeded__ = default(bool);
            var __ErrorCode__ = default(int);
            var __ErrorDesc__ = default(string);
            var __PurchasedData__ = default(byte[]);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __IsSucceeded__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 3:
                        __ErrorCode__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 4:
                        __ErrorDesc__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __PurchasedData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.SteamFinalizeTransactionRes();
            ____result.IsSucceeded = __IsSucceeded__;
            ____result.ErrorCode = __ErrorCode__;
            ____result.ErrorDesc = __ErrorDesc__;
            ____result.PurchasedData = __PurchasedData__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class SteamInitTransactionReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.SteamInitTransactionReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.SteamInitTransactionReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += MessagePackBinary.WriteUInt64(ref bytes, offset, value.SteamId);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Sku, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.SteamInitTransactionReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __SteamId__ = default(ulong);
            var __Sku__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __SteamId__ = MessagePackBinary.ReadUInt64(bytes, offset, out readSize);
                        break;
                    case 2:
                        __Sku__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.SteamInitTransactionReq();
            ____result.SteamId = __SteamId__;
            ____result.Sku = __Sku__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class SteamInitTransactionResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.SteamInitTransactionRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.SteamInitTransactionRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 8);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsSucceeded);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.OrderId);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.TransId);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.SteamUrl, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.ErrorCode);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ErrorDesc, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.SteamInitTransactionRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __IsSucceeded__ = default(bool);
            var __OrderId__ = default(long);
            var __TransId__ = default(long);
            var __SteamUrl__ = default(string);
            var __ErrorCode__ = default(int);
            var __ErrorDesc__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __IsSucceeded__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 3:
                        __OrderId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __TransId__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 5:
                        __SteamUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __ErrorCode__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __ErrorDesc__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.SteamInitTransactionRes();
            ____result.IsSucceeded = __IsSucceeded__;
            ____result.OrderId = __OrderId__;
            ____result.TransId = __TransId__;
            ____result.SteamUrl = __SteamUrl__;
            ____result.ErrorCode = __ErrorCode__;
            ____result.ErrorDesc = __ErrorDesc__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class ShopProductListReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ShopProductListReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ShopProductListReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref bytes, offset, value.Skus, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ShopProductListReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Skus__ = default(global::System.Collections.Generic.List<string>);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Skus__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ShopProductListReq();
            ____result.Skus = __Skus__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class ShopProductInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ShopProductInfo>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ShopProductInfo value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ProductId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Title, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Price, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Description, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ShopProductInfo Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProductId__ = default(string);
            var __Title__ = default(string);
            var __Price__ = default(string);
            var __Description__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProductId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Title__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Price__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __Description__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ShopProductInfo();
            ____result.ProductId = __ProductId__;
            ____result.Title = __Title__;
            ____result.Price = __Price__;
            ____result.Description = __Description__;
            return ____result;
        }
    }


    public sealed class ShopProductListResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ShopProductListRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ShopProductListRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.ShopProductInfo>>().Serialize(ref bytes, offset, value.Products, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ShopProductListRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Products__ = default(global::System.Collections.Generic.List<global::HaeginGame.ShopProductInfo>);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Products__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.ShopProductInfo>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ShopProductListRes();
            ____result.Products = __Products__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class MaintenanceCheckReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.MaintenanceCheckReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.MaintenanceCheckReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ServerName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.MaintenanceCheckReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ServerName__ = default(string);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __ServerName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.MaintenanceCheckReq();
            ____result.ServerName = __ServerName__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class MaintenanceCheckResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.MaintenanceCheckRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.MaintenanceCheckRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 8);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsMaintenance);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.StartTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.EndTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ServerUrl, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PatchUrl, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Message, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.MaintenanceCheckRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __IsMaintenance__ = default(bool);
            var __StartTime__ = default(global::System.DateTime);
            var __EndTime__ = default(global::System.DateTime);
            var __ServerUrl__ = default(string);
            var __PatchUrl__ = default(string);
            var __Message__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __IsMaintenance__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 3:
                        __StartTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __EndTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __ServerUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __PatchUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __Message__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.MaintenanceCheckRes();
            ____result.IsMaintenance = __IsMaintenance__;
            ____result.StartTime = __StartTime__;
            ____result.EndTime = __EndTime__;
            ____result.ServerUrl = __ServerUrl__;
            ____result.PatchUrl = __PatchUrl__;
            ____result.Message = __Message__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class NoticeReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.NoticeReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.NoticeReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Lang, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.NoticeReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Lang__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Lang__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.NoticeReq();
            ____result.Lang = __Lang__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class NoticeResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.NoticeRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.NoticeRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Url, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.UpdateTime, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.NoticeRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Url__ = default(string);
            var __UpdateTime__ = default(global::System.DateTime);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Url__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __UpdateTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.NoticeRes();
            ____result.Url = __Url__;
            ____result.UpdateTime = __UpdateTime__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class KeyCountReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.KeyCountReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.KeyCountReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Key, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.KeyCountReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Key__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Key__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.KeyCountReq();
            ____result.Key = __Key__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class KeyCountResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.KeyCountRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.KeyCountRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.KeyCountRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.KeyCountRes();
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class CouponReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.CouponReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.CouponReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 2);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Coupon, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.CouponReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Coupon__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Coupon__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.CouponReq();
            ____result.Coupon = __Coupon__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class CouponResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.CouponRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.CouponRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.Data, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.CouponRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Data__ = default(byte[]);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Data__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.CouponRes();
            ____result.Data = __Data__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class MaintenanceCheckV2ReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.MaintenanceCheckV2Req>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.MaintenanceCheckV2Req value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ServerName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.MaintenanceCheckV2Req Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __ServerName__ = default(string);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __ServerName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.MaintenanceCheckV2Req();
            ____result.ServerName = __ServerName__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class MaintenanceCheckV2ResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.MaintenanceCheckV2Res>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.MaintenanceCheckV2Res value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 9);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += MessagePackBinary.WriteBoolean(ref bytes, offset, value.IsMaintenance);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.StartTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref bytes, offset, value.EndTime, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.CommonUrl, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.GameUrl, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PatchUrl, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Message, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.MaintenanceCheckV2Res Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __IsMaintenance__ = default(bool);
            var __StartTime__ = default(global::System.DateTime);
            var __EndTime__ = default(global::System.DateTime);
            var __CommonUrl__ = default(string);
            var __GameUrl__ = default(string);
            var __PatchUrl__ = default(string);
            var __Message__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __IsMaintenance__ = MessagePackBinary.ReadBoolean(bytes, offset, out readSize);
                        break;
                    case 3:
                        __StartTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 4:
                        __EndTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __CommonUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __GameUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 7:
                        __PatchUrl__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 8:
                        __Message__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.MaintenanceCheckV2Res();
            ____result.IsMaintenance = __IsMaintenance__;
            ____result.StartTime = __StartTime__;
            ____result.EndTime = __EndTime__;
            ____result.CommonUrl = __CommonUrl__;
            ____result.GameUrl = __GameUrl__;
            ____result.PatchUrl = __PatchUrl__;
            ____result.Message = __Message__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }


    public sealed class OneStorePurchasedProductFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.OneStorePurchasedProduct>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.OneStorePurchasedProduct value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 9);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.OrderId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PackageName, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.ProductId, formatterResolver);
            offset += MessagePackBinary.WriteInt64(ref bytes, offset, value.PurchaseTime);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.PurchaseId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.DeveloperPayload, formatterResolver);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.PurchaseState);
            offset += MessagePackBinary.WriteInt32(ref bytes, offset, value.RecurringState);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumptionState>().Serialize(ref bytes, offset, value.ConsumptionState, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.OneStorePurchasedProduct Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __OrderId__ = default(string);
            var __PackageName__ = default(string);
            var __ProductId__ = default(string);
            var __PurchaseTime__ = default(long);
            var __PurchaseId__ = default(string);
            var __DeveloperPayload__ = default(string);
            var __PurchaseState__ = default(int);
            var __RecurringState__ = default(int);
            var __ConsumptionState__ = default(global::HaeginGame.ConsumptionState);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __OrderId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __PackageName__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __ProductId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __PurchaseTime__ = MessagePackBinary.ReadInt64(bytes, offset, out readSize);
                        break;
                    case 4:
                        __PurchaseId__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 5:
                        __DeveloperPayload__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 6:
                        __PurchaseState__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 7:
                        __RecurringState__ = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
                        break;
                    case 8:
                        __ConsumptionState__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ConsumptionState>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.OneStorePurchasedProduct();
            ____result.OrderId = __OrderId__;
            ____result.PackageName = __PackageName__;
            ____result.ProductId = __ProductId__;
            ____result.PurchaseTime = __PurchaseTime__;
            ____result.PurchaseId = __PurchaseId__;
            ____result.DeveloperPayload = __DeveloperPayload__;
            ____result.PurchaseState = __PurchaseState__;
            ____result.RecurringState = __RecurringState__;
            ____result.ConsumptionState = __ConsumptionState__;
            return ____result;
        }
    }


    public sealed class ConsumeOneStoreReceiptReqFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumeOneStoreReceiptReq>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumeOneStoreReceiptReq value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 3);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>>().Serialize(ref bytes, offset, value.Products, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<string>().Serialize(ref bytes, offset, value.Language, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ConsumeOneStoreReceiptReq Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Products__ = default(global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>);
            var __Language__ = default(string);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 1:
                        __Products__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 2:
                        __Language__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ConsumeOneStoreReceiptReq();
            ____result.Products = __Products__;
            ____result.Language = __Language__;
            ____result.ProtocolId = __ProtocolId__;
            return ____result;
        }
    }


    public sealed class ConsumeOneStoreReceiptResFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::HaeginGame.ConsumeOneStoreReceiptRes>
    {

        public int Serialize(ref byte[] bytes, int offset, global::HaeginGame.ConsumeOneStoreReceiptRes value, global::MessagePack.IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return global::MessagePack.MessagePackBinary.WriteNil(ref bytes, offset);
            }
            
            var startOffset = offset;
            offset += global::MessagePack.MessagePackBinary.WriteFixedArrayHeaderUnsafe(ref bytes, offset, 4);
            offset += formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Serialize(ref bytes, offset, value.ProtocolId, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Serialize(ref bytes, offset, value.Result, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>>().Serialize(ref bytes, offset, value.Products, formatterResolver);
            offset += formatterResolver.GetFormatterWithVerify<byte[]>().Serialize(ref bytes, offset, value.PurchasedData, formatterResolver);
            return offset - startOffset;
        }

        public global::HaeginGame.ConsumeOneStoreReceiptRes Deserialize(byte[] bytes, int offset, global::MessagePack.IFormatterResolver formatterResolver, out int readSize)
        {
            if (global::MessagePack.MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var startOffset = offset;
            var length = global::MessagePack.MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            offset += readSize;

            var __Products__ = default(global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>);
            var __PurchasedData__ = default(byte[]);
            var __ProtocolId__ = default(global::HaeginGame.ProtocolId);
            var __Result__ = default(global::Haegin.Result);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        __Products__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::HaeginGame.OneStorePurchasedProduct>>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 3:
                        __PurchasedData__ = formatterResolver.GetFormatterWithVerify<byte[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 0:
                        __ProtocolId__ = formatterResolver.GetFormatterWithVerify<global::HaeginGame.ProtocolId>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    case 1:
                        __Result__ = formatterResolver.GetFormatterWithVerify<global::Haegin.Result>().Deserialize(bytes, offset, formatterResolver, out readSize);
                        break;
                    default:
                        readSize = global::MessagePack.MessagePackBinary.ReadNextBlock(bytes, offset);
                        break;
                }
                offset += readSize;
            }

            readSize = offset - startOffset;

            var ____result = new global::HaeginGame.ConsumeOneStoreReceiptRes();
            ____result.Products = __Products__;
            ____result.PurchasedData = __PurchasedData__;
            ____result.ProtocolId = __ProtocolId__;
            ____result.Result = __Result__;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
