import { authService, shippingService } from "../services/api";

export const calculateShippingOptions = async (zipCode, cartItems) => {
  if (!zipCode || zipCode.length < 8) {
    throw new Error("Preencha o CEP");
  }

  const shippingData = {
    productIds: cartItems.map((item) => item.product.id),
    quantities: cartItems.map((item) => item.quantity),
    destinationZipCode: zipCode.replace(/\D/g, ""),
  };

  const response = await shippingService.calculateShipping(shippingData);

  if (response.data.success) {
    return response.data.options;
  } else {
    throw new Error(response.data.error || "Erro desconhecido");
  }
};

export const loadUserProfile = async (setFormData) => {
  try {
    const response = await authService.getProfile();
    const profileData = response.data;

    setFormData((prev) => ({
      ...prev,
      firstName: profileData.firstName || "",
      lastName: profileData.lastName || "",
      email: profileData.email || "",
      cardholderName:
        profileData.firstName && profileData.lastName
          ? `${profileData.firstName} ${profileData.lastName}`
          : "",
    }));
  } catch {
    alert("Erro ao carregar perfil.");
  }
};

export const validateFormData = (formData) => {
  const newErrors = {};

  const requiredFields = [
    "firstName",
    "lastName",
    "email",
    "phone",
    "address",
    "city",
    "state",
    "zipCode",
    "cardNumber",
    "expiryDate",
    "cvv",
    "cardholderName",
  ];

  requiredFields.forEach((field) => {
    if (!formData[field].trim()) {
      newErrors[field] = "Este campo é obrigatório";
    }
  });

  if (formData.email && !/\S+@\S+\.\S+/.test(formData.email)) {
    newErrors.email = "Insira um endereço de email válido";
  }

  if (formData.phone && !/^\+?[\d\s\-()]{10,}$/.test(formData.phone)) {
    newErrors.phone = "Insira um número de telefone válido";
  }

  if (
    formData.cardNumber &&
    !/^\d{13,19}$/.test(formData.cardNumber.replace(/\s/g, ""))
  ) {
    newErrors.cardNumber = "Insira um número de cartão válido";
  }

  if (
    formData.expiryDate &&
    !/^(0[1-9]|1[0-2])\/\d{2}$/.test(formData.expiryDate)
  ) {
    newErrors.expiryDate = "Insira a data no formato MM/AA";
  }

  if (formData.cvv && !/^\d{3,4}$/.test(formData.cvv)) {
    newErrors.cvv = "Insira um CVV válido";
  }

  return newErrors;
};

export const getShippingDisplayText = (
  selectedShipping,
  shippingCalculated
) => {
  if (selectedShipping) {
    return `R$ ${selectedShipping.price.toFixed(2)} - ${
      selectedShipping.serviceName
    }`;
  }
  return shippingCalculated
    ? "Selecione uma opção de entrega"
    : "Informe o CEP";
};

export const handleZipCodeChange = (
  zipCode,
  calculateShipping,
  setShippingOptions,
  setSelectedShipping,
  setShippingCalculated
) => {
  const cleanZip = zipCode.replace(/\D/g, "");
  if (cleanZip.length === 8) {
    calculateShipping();
  } else {
    setShippingOptions([]);
    setSelectedShipping(null);
    setShippingCalculated(false);
  }
};

export const validateSubmission = (
  cartItems,
  shippingCalculated,
  selectedShipping
) => {
  if (cartItems.length === 0) {
    throw new Error("Seu carrinho está vazio");
  }

  if (!shippingCalculated || !selectedShipping) {
    throw new Error(
      "Calcule e selecione uma opção de frete antes de finalizar o pedido"
    );
  }
};

export const createOrderData = (formData, cartItems, selectedShipping) => {
  return {
    customerName: `${formData.firstName} ${formData.lastName}`,
    customerEmail: formData.email,
    shippingAddress: `${formData.address}, ${formData.city}, ${formData.state} ${formData.zipCode}, ${formData.country}`,
    items: cartItems.map((item) => ({
      productId: item.product.id,
      quantity: item.quantity,
      unitPrice: item.product.price,
    })),
    shippingCost: selectedShipping.price,
    shippingService: selectedShipping.serviceName,
    shippingDays: selectedShipping.deliveryDays,
  };
};
