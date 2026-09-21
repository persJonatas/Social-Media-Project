import axios from 'axios'
import { USERS_API, CIS_API } from './config'

const usersApi = axios.create({ baseURL: USERS_API })
const cisApi   = axios.create({ baseURL: CIS_API })

cisApi.interceptors.request.use(cfg => {
  const token = localStorage.getItem('token')
  if (token) cfg.headers.Authorization = `Bearer ${token}`
  return cfg
})

const handle = err => {
  const status = err?.response?.status
  if (status === 401) throw new Error('Não autorizado. Faça login novamente.')
  if (status === 404) throw new Error('Recurso não encontrado.')
  if (status === 403) throw new Error('Você não tem permissão para esta ação.')
  if (status === 409) throw new Error('Conflito: operação já realizada.')
  throw new Error(err?.response?.data?.error || 'Erro inesperado.')
}

export const validateToken = async (token) => {
  try {
    const { data } = await usersApi.post('/auth/validate', { token });
    return data;
  } catch (e) { handle(e); }
}

export const login = async (login, password) => {
  try {
    const { data } = await usersApi.post('/auth/login', { login, password })
    return data
  } catch (e) { handle(e) }
}

export const register = async (name, login, password) => {
  try {
    const { data } = await usersApi.post('/users', { name, login, password })
    return data
  } catch (e) { handle(e) }
}

export const getTopics = async () => {
  try {
    const { data } = await cisApi.get('/topics')
    return data
  } catch (e) { handle(e) }
}

export const createTopic = async (title, description) => {
  try {
    const { data } = await cisApi.post('/topics', { title, description })
    return data
  } catch (e) { handle(e) }
}

export const updateTopic = async (id, title, description) => {
  try {
    const { data } = await cisApi.put(`/topics/${id}`, { title, description });
    return data;
  } catch (e) { handle(e); }
};

export const deleteTopic = async (topicId) => {
  try {
    await cisApi.delete(`/topics/${topicId}`) 
  } catch (e) { handle(e) }
}

export const getIdeasByTopic = async (topicId) => {
  try {
    const { data } = await cisApi.get(`/topics/${topicId}/ideas`)
    return data
  } catch (e) { handle(e) }
}

export const createIdea = async (topicId, title, description) => {
  try {
    const { data } = await cisApi.post(`/topics/${topicId}/ideas`, { title, description })
    return data
  } catch (e) { handle(e) }
}

export const updateIdea = async (ideaId, title, description) => {
  try {
    const { data } = await cisApi.put(`/ideas/${ideaId}`, { title, description })
    return data
  } catch (e) { handle(e) }
}

export const deleteIdea = async (ideaId) => {
  try {
    await cisApi.delete(`/ideas/${ideaId}`)
  } catch (e) { handle(e) }
}

export const castVote = async (ideaId) => {
  try {
    const { data } = await cisApi.post(`/ideas/${ideaId}/votes`, { ideaId })
    return data
  } catch (e) { handle(e) }
}

export const cancelVote = async (ideaId) => {
  try {
    await cisApi.delete(`/ideas/${ideaId}/votes`)
  } catch (e) { handle(e) }
}