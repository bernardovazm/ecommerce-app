import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { orderService } from "../services/api";

const OrderConfirmationPage = () => {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        setLoading(true);
        const response = await orderService.getById(orderId);
        console.log("Order response:", response);
        setOrder(response.data);
      } catch (err) {
        setError("Falha ao carregar detalhes do pedido");
        console.error("Error fetching order:", err);
      } finally {
        setLoading(false);
      }
    };

    if (orderId) {
      fetchOrder();
    }
  }, [orderId]);

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-center items-center min-h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">
            Pedido Não Encontrado
          </h2>
          <p className="text-gray-600 mb-4">
            {error || "O pedido que você está procurando não existe."}
          </p>
          <button
            onClick={() => navigate("/")}
            className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Ir para a Página Inicial
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-3xl mx-auto">
        <div className="text-center mb-8">
          <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
            <svg
              className="h-6 w-6 text-green-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            Pedido Confirmado!
          </h1>
          <p className="text-gray-600">
            Obrigado pela sua compra. Seu pedido foi realizado com sucesso.
          </p>
        </div>
        <div className="bg-white rounded-lg shadow-sm border p-6 mb-6">
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-2">
                Pedido #{order.id}
              </h2>
              <p className="text-gray-600">
                Realizado em{" "}
                {new Date(order.createdAt).toLocaleDateString("pt-BR", {
                  year: "numeric",
                  month: "long",
                  day: "numeric",
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </p>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-sm font-medium ${
                order.status === "Pending"
                  ? "bg-yellow-100 text-yellow-800"
                  : order.status === "Processing"
                  ? "bg-blue-100 text-blue-800"
                  : order.status === "Shipped"
                  ? "bg-purple-100 text-purple-800"
                  : order.status === "Delivered"
                  ? "bg-green-100 text-green-800"
                  : "bg-gray-100 text-gray-800"
              }`}
            >
              {order.status === "Pending"
                ? "Pendente"
                : order.status === "Processing"
                ? "Processando"
                : order.status === "Shipped"
                ? "Enviado"
                : order.status === "Delivered"
                ? "Entregue"
                : order.status}
            </span>
          </div>
          <div className="border-t pt-6">
            <h3 className="text-lg font-medium text-gray-900 mb-4">
              Itens do Pedido
            </h3>
            <div className="space-y-4">
              {order.items &&
                order.items.map((item, index) => (
                  <div
                    key={index}
                    className="flex justify-between items-center py-2 border-b last:border-b-0"
                  >
                    <div className="flex-1">
                      <h4 className="font-medium text-gray-900">
                        {item.productName || `Produto ${item.id || "Genérico"}`}
                      </h4>
                      <p className="text-sm text-gray-600">
                        Quantidade: {item.quantity || 0}
                      </p>
                      <p className="text-sm text-gray-600">
                        Preço: R$ {(item.unitPrice || 0).toFixed(2)} cada
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="font-medium text-gray-900">
                        R$
                        {((item.unitPrice || 0) * (item.quantity || 0)).toFixed(
                          2
                        )}
                      </p>
                    </div>
                  </div>
                ))}
            </div>
          </div>
          <div className="border-t pt-6 mt-6">
            <div className="flex justify-between items-center">
              <span className="text-lg font-semibold text-gray-900">Total</span>
              <span className="text-lg font-semibold text-gray-900">
                R$ {(order.total || 0).toFixed(2)}
              </span>
            </div>
          </div>
        </div>
        {order.shippingAddress && (
          <div className="bg-white rounded-lg shadow-sm border p-6 mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Endereço de Entrega
            </h2>
            <div className="text-gray-600">
              <p>{order.customer?.name}</p>
              <p>{order.shippingAddress.address}</p>
              <p>
                {order.shippingAddress.city}, {order.shippingAddress.state}
                {order.shippingAddress.zipCode}
              </p>
              <p>{order.shippingAddress.country}</p>
            </div>
          </div>
        )}
        {order.customer && (
          <div className="bg-white rounded-lg shadow-sm border p-6 mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Informações do Cliente
            </h2>
            <div className="text-gray-600">
              <p>
                <strong>Nome:</strong> {order.customer?.fullName}
              </p>
              <p>
                <strong>Email:</strong> {order.customer?.email}
              </p>
              {order.customer?.phone && (
                <p>
                  <strong>Telefone:</strong> {order.customer.phone}
                </p>
              )}
            </div>
          </div>
        )}
        <div className="bg-blue-50 rounded-lg p-6 mb-6">
          <h2 className="text-lg font-semibold text-blue-900 mb-3">
            Próximos Passos
          </h2>
          <ul className="text-blue-800 space-y-2">
            <li className="flex items-start">
              <span className="mr-2">•</span>
              <span>
                Você receberá um email de confirmação em breve com os detalhes
                do seu pedido.
              </span>
            </li>
            <li className="flex items-start">
              <span className="mr-2">•</span>
              <span>
                Enviaremos informações de rastreamento assim que seu pedido for
                enviado.
              </span>
            </li>
            <li className="flex items-start">
              <span className="mr-2">•</span>
              <span>Tempo estimado de entrega é de 3-5 dias úteis.</span>
            </li>
          </ul>
        </div>
        <div className="flex flex-col sm:flex-row gap-4">
          <button
            onClick={() => navigate("/products")}
            className="flex-1 bg-blue-600 text-white py-3 px-6 rounded-lg font-medium hover:bg-blue-700 transition-colors"
          >
            Continuar Comprando
          </button>
          <button
            onClick={() => navigate("/")}
            className="flex-1 border border-gray-300 text-gray-700 py-3 px-6 rounded-lg font-medium hover:bg-gray-50 transition-colors"
          >
            Ir para a Página Inicial
          </button>
        </div>
      </div>
    </div>
  );
};

export default OrderConfirmationPage;
