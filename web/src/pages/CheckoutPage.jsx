import { useState, useEffect, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useCart } from "../hooks/useCart";
import { useAuth } from "../hooks/useAuth";
import { orderService } from "../services/api";
import ShippingSection from "../components/ShippingSection";
import { validateFormData, createOrderData, loadUserProfile, calculateShippingOptions, validateSubmission, handleZipCodeChange, getShippingDisplayText } from "../utils/checkoutHelpers";
import EmptyCartMessage from "../components/EmptyCartMessage";

const CheckoutPage = () => {
  const navigate = useNavigate();
  const { cartItems, getCartTotal, clearCart } = useCart();
  const { user } = useAuth();
  const [loading, setLoading] = useState(false); const [shippingOptions, setShippingOptions] = useState([]);
  const [selectedShipping, setSelectedShipping] = useState(null); const [shippingCalculated, setShippingCalculated] = useState(false);
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    address: "",
    city: "",
    state: "",
    zipCode: "",
    country: "US",
    cardNumber: "",
    expiryDate: "",
    cvv: "",
    cardholderName: "",
    notes: "",
  }); const [errors, setErrors] = useState({}); useEffect(() => {
    if (user) {
      loadUserProfile(setFormData);
    }
  }, [user]);
  const calculateShipping = useCallback(async () => {
    setShippingOptions([]);
    setSelectedShipping(null);
    setShippingCalculated(false);

    try {
      const options = await calculateShippingOptions(formData.zipCode, cartItems);
      setShippingOptions(options);
      setShippingCalculated(true);
      if (options.length > 0) {
        setSelectedShipping(options[0]);
      }
    } catch (error) {
      alert("Erro ao calcular frete: " + error.message);
      setShippingCalculated(false);
    }
  }, [formData.zipCode, cartItems]);
  useEffect(() => {
    handleZipCodeChange(formData.zipCode, calculateShipping, setShippingOptions, setSelectedShipping, setShippingCalculated);
  }, [formData.zipCode, calculateShipping]);

  const getShippingCost = () => {
    return selectedShipping?.price || 0;
  };

  const getFinalTotal = () => {
    return getCartTotal() + getShippingCost();
  };
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    if (errors[name]) {
      setErrors((prev) => ({
        ...prev,
        [name]: "",
      }));
    }
  };

  const handleShippingSelection = (option) => {
    setSelectedShipping(option);
  };
  const validateForm = () => {
    const newErrors = validateFormData(formData);
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }; const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      validateSubmission(cartItems, shippingCalculated, selectedShipping);
    } catch (error) {
      alert(error.message);
      return;
    }

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      const orderData = createOrderData(formData, cartItems, selectedShipping);
      const response = await orderService.createGuest(orderData);
      clearCart();
      navigate(`/order-confirmation/${response.data.orderId}`);
    } catch (error) {
      alert("Falha ao processar o pedido: " + error);
      return;
    } finally {
      setLoading(false);
    }
  };
  if (cartItems.length === 0) {
    return <EmptyCartMessage navigate={navigate} />;
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">
        Finalizar Compra
      </h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2">

          <form onSubmit={handleSubmit} className="space-y-8" id="checkout-form">
            <div className="bg-white rounded-lg shadow-sm border p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Informações do Cliente
              </h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label
                    htmlFor="firstName"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Nome *
                  </label>
                  <input
                    type="text"
                    id="firstName"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.firstName ? "border-red-500" : "border-gray-300"
                      }`}
                  />
                  {errors.firstName && (
                    <p className="text-red-500 text-sm mt-1">
                      {errors.firstName}
                    </p>
                  )}
                </div>
                <div>
                  <label
                    htmlFor="lastName"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Sobrenome *
                  </label>
                  <input
                    type="text"
                    id="lastName"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.lastName ? "border-red-500" : "border-gray-300"
                      }`}
                  />
                  {errors.lastName && (
                    <p className="text-red-500 text-sm mt-1">
                      {errors.lastName}
                    </p>
                  )}
                </div>
                <div>
                  <label
                    htmlFor="email"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Endereço de Email *
                  </label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    value={formData.email}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.email ? "border-red-500" : "border-gray-300"
                      }`}
                  />
                  {errors.email && (
                    <p className="text-red-500 text-sm mt-1">{errors.email}</p>
                  )}
                </div>
                <div>
                  <label
                    htmlFor="phone"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Número de Telefone *
                  </label>
                  <input
                    type="tel"
                    id="phone"
                    name="phone"
                    value={formData.phone}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.phone ? "border-red-500" : "border-gray-300"
                      }`}
                  />
                  {errors.phone && (
                    <p className="text-red-500 text-sm mt-1">{errors.phone}</p>
                  )}
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Endereço de Entrega
              </h2>
              <div className="space-y-4">
                <div>
                  <label
                    htmlFor="address"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Endereço *
                  </label>
                  <input
                    type="text"
                    id="address"
                    name="address"
                    value={formData.address}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.address ? "border-red-500" : "border-gray-300"
                      }`}
                  />
                  {errors.address && (
                    <p className="text-red-500 text-sm mt-1">
                      {errors.address}
                    </p>
                  )}
                </div>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label
                      htmlFor="city"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Cidade *
                    </label>
                    <input
                      type="text"
                      id="city"
                      name="city"
                      value={formData.city}
                      onChange={handleInputChange}
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.city ? "border-red-500" : "border-gray-300"
                        }`}
                    />
                    {errors.city && (
                      <p className="text-red-500 text-sm mt-1">{errors.city}</p>
                    )}
                  </div>
                  <div>
                    <label
                      htmlFor="state"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Estado *
                    </label>
                    <input
                      type="text"
                      id="state"
                      name="state"
                      value={formData.state}
                      onChange={handleInputChange}
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.state ? "border-red-500" : "border-gray-300"
                        }`}
                    />
                    {errors.state && (
                      <p className="text-red-500 text-sm mt-1">
                        {errors.state}
                      </p>
                    )}
                  </div>
                  <div>
                    <label
                      htmlFor="zipCode"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      CEP *
                    </label>
                    <div className="flex gap-2">
                      <input
                        type="text"
                        id="zipCode"
                        name="zipCode"
                        value={formData.zipCode}
                        onChange={handleInputChange}
                        placeholder="00000-000"
                        className={`flex-1 px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.zipCode ? "border-red-500" : "border-gray-300"
                          }`}
                      />
                    </div>
                    {errors.zipCode && (
                      <p className="text-red-500 text-sm mt-1">
                        {errors.zipCode}
                      </p>)}
                  </div>
                </div>
              </div>            </div>

            <ShippingSection
              shippingCalculated={shippingCalculated}
              shippingOptions={shippingOptions}
              selectedShipping={selectedShipping}
              handleShippingSelection={handleShippingSelection}
            />

            <div className="bg-white rounded-lg shadow-sm border p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Informações de Pagamento
              </h2>
              <div className="space-y-4">
                <div>
                  <label
                    htmlFor="cardholderName"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Nome do Titular *
                  </label>
                  <input
                    type="text"
                    id="cardholderName"
                    name="cardholderName"
                    value={formData.cardholderName}
                    onChange={handleInputChange}
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.cardholderName
                      ? "border-red-500"
                      : "border-gray-300"
                      }`}
                  />
                  {errors.cardholderName && (
                    <p className="text-red-500 text-sm mt-1">
                      {errors.cardholderName}
                    </p>
                  )}
                </div>
                <div>
                  <label
                    htmlFor="cardNumber"
                    className="block text-sm font-medium text-gray-700 mb-1"
                  >
                    Número do Cartão *
                  </label>
                  <input
                    type="text"
                    id="cardNumber"
                    name="cardNumber"
                    value={formData.cardNumber}
                    onChange={handleInputChange}
                    placeholder="1234 5678 9012 3456"
                    className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.cardNumber ? "border-red-500" : "border-gray-300"
                      }`}
                  />
                  {errors.cardNumber && (
                    <p className="text-red-500 text-sm mt-1">
                      {errors.cardNumber}
                    </p>
                  )}
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label
                      htmlFor="expiryDate"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Data de Validade *
                    </label>
                    <input
                      type="text"
                      id="expiryDate"
                      name="expiryDate"
                      value={formData.expiryDate}
                      onChange={handleInputChange}
                      placeholder="MM/AA"
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.expiryDate ? "border-red-500" : "border-gray-300"
                        }`}
                    />
                    {errors.expiryDate && (
                      <p className="text-red-500 text-sm mt-1">
                        {errors.expiryDate}
                      </p>
                    )}
                  </div>
                  <div>
                    <label
                      htmlFor="cvv"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      CVV *
                    </label>
                    <input
                      type="text"
                      id="cvv"
                      name="cvv"
                      value={formData.cvv}
                      onChange={handleInputChange}
                      placeholder="123"
                      className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent ${errors.cvv ? "border-red-500" : "border-gray-300"
                        }`}
                    />
                    {errors.cvv && (
                      <p className="text-red-500 text-sm mt-1">{errors.cvv}</p>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-sm border p-6">
              <h2 className="text-xl font-semibold text-gray-900 mb-4">
                Observações do Pedido (Opcional)
              </h2>
              <textarea
                id="notes"
                name="notes"
                value={formData.notes}
                onChange={handleInputChange}
                rows={3}
                placeholder="Instruções especiais para seu pedido..."
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </form>
        </div>

        <div className="lg:col-span-1">
          <div className="bg-white rounded-lg shadow-sm border p-6 sticky top-4">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Resumo do Pedido
            </h2>
            <div className="space-y-3 mb-6">
              {cartItems.map((item) => (
                <div
                  key={item.product.id}
                  className="flex justify-between items-center"
                >
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">
                      {item.product.name}
                    </p>
                    <p className="text-sm text-gray-600">
                      Qtd: {item.quantity}
                    </p>
                  </div>
                  <p className="text-sm font-medium text-gray-900">
                    R$ {(item.product.price * item.quantity).toFixed(2)}
                  </p>
                </div>
              ))}
            </div>
            <div className="space-y-2 mb-6 pt-4 border-t">
              <div className="flex justify-between">
                <span className="text-gray-600">Subtotal</span>
                <span className="font-medium">
                  R$ {getCartTotal().toFixed(2)}
                </span>
              </div>              <div className="flex justify-between">
                <span className="text-gray-600">Frete</span>
                <span className="font-medium">
                  {getShippingDisplayText(selectedShipping, shippingCalculated)}
                </span>
              </div>
              <div className="flex justify-between pt-2 border-t">                <span className="text-lg font-semibold text-gray-900">
                Total
              </span>
                <span className="text-lg font-semibold text-gray-900">
                  {selectedShipping ? `R$ ${getFinalTotal().toFixed(2)}` : "--"}
                </span>
              </div>
            </div>
            <button
              type="submit"
              form="checkout-form"
              onClick={handleSubmit}
              disabled={loading}
              className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {loading ? (
                <div className="flex items-center justify-center">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                  Processando...
                </div>
              ) : (
                "Finalizar Pedido"
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CheckoutPage;
