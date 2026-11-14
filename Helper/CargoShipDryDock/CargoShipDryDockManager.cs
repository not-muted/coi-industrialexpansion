using System;
using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Cargo.Modules;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Products;
using Mafi.Core.Simulation;
using Mafi.Serialization;

namespace IndustrialExpansion.Helper.CargoShipDryDock
{
    [GlobalDependency(RegistrationMode.AsEverything, false, false)]
    public class CargoShipDryDockManager : IVirtualBufferProvider
    {
        public ImmutableArray<ProductProto> ProvidedProducts { get; private set; }

        public CargoShipDryDockManager(CargoDepotManager depotManager, ISimLoopEvents simLoopEvents, [ProtoDep("Product_Virtual_CargoShip")] ProductProto cargoShip)
        {
            this.m_depotManager = depotManager;
            this.m_simLoopEvents = simLoopEvents;
            this.m_cargoShip = cargoShip;
            this.m_cargoShipBuffer = new ProductBuffer(Quantity.MaxValue, this.m_cargoShip);
            this.ProvidedProducts = ImmutableArray.Create<ProductProto>(this.m_cargoShip);
            simLoopEvents.Update.Add<CargoShipDryDockManager>(this, new Action(this.checkForNewShips));
        }

        private void checkForNewShips()
        {
            bool isNotEmpty = this.m_cargoShipBuffer.IsNotEmpty;
            if (isNotEmpty)
            {
                Quantity quantity = this.m_cargoShipBuffer.RemoveAsMuchAs(this.m_cargoShipBuffer.Quantity);
                this.m_depotManager.ReportNewCargoShipFound(quantity.Value);
                this.m_depotManager.ReportNewCargoShipRepaired(quantity.Value);
            }
        }

        public static void Serialize(CargoShipDryDockManager value, BlobWriter writer)
        {
            bool flag = writer.TryStartClassSerialization<CargoShipDryDockManager>(value);
            if (flag)
            {
                writer.EnqueueDataSerialization(value, CargoShipDryDockManager.s_serializeDataDelayedAction);
            }
        }

        protected virtual void SerializeData(BlobWriter writer)
        {
            writer.WriteGeneric<ProductProto>(this.m_cargoShip);
            ProductBuffer.Serialize(this.m_cargoShipBuffer, writer);
            ImmutableArray<ProductProto>.Serialize(this.ProvidedProducts, writer);
            CargoDepotManager.Serialize(this.m_depotManager, writer);
            writer.WriteGeneric<ISimLoopEvents>(this.m_simLoopEvents);
        }

        public static CargoShipDryDockManager Deserialize(BlobReader reader)
        {
            CargoShipDryDockManager CargoShipDryDockManager;
            bool flag = reader.TryStartClassDeserialization<CargoShipDryDockManager>(out CargoShipDryDockManager, null, null, false);
            if (flag)
            {
                reader.EnqueueDataDeserialization(CargoShipDryDockManager, CargoShipDryDockManager.s_deserializeDataDelayedAction, null);
            }
            return CargoShipDryDockManager;
        }

        protected virtual void DeserializeData(BlobReader reader)
        {
            reader.SetField<CargoShipDryDockManager>(this, "m_cargoShip", reader.ReadGenericAs<ProductProto>());
            reader.SetField<CargoShipDryDockManager>(this, "m_cargoShipBuffer", ProductBuffer.Deserialize(reader));
            this.ProvidedProducts = ImmutableArray<ProductProto>.Deserialize(reader);
            reader.SetField<CargoShipDryDockManager>(this, "m_depotManager", CargoDepotManager.Deserialize(reader));
            reader.SetField<CargoShipDryDockManager>(this, "m_simLoopEvents", reader.ReadGenericAs<ISimLoopEvents>());
        }

        public Option<IPlainProductBuffer> GetBuffer(ProductProto product, Option<IEntity> entity)
        {
            bool flag = product == this.m_cargoShip;
            Option<IPlainProductBuffer> option;
            if (flag)
            {
                option = this.m_cargoShipBuffer;
            }
            else
            {
                option = Option<IPlainProductBuffer>.None;
            }
            return option;
        }

        private readonly CargoDepotManager m_depotManager;

        private readonly ISimLoopEvents m_simLoopEvents;

        private readonly ProductProto m_cargoShip;

        private readonly ProductBuffer m_cargoShipBuffer;

        private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
        {
            ((CargoShipDryDockManager)obj).SerializeData(writer);
        };

        private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
        {
            ((CargoShipDryDockManager)obj).DeserializeData(reader);
        };
    }
}
